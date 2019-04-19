namespace Net7MultiClientUnlocker
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Management;
    using System.Security.Principal;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Microsoft.Win32;
    using Net7MultiClientUnlocker.Domain;
    using Net7MultiClientUnlocker.Framework;
    using Net7MultiClientUnlocker.Framework.Win32;

    public partial class MainWindow
    {
        private readonly NotifyingDataContext dataContext;
        private readonly ManagementEventWatcher startWatcher;
        private readonly ManagementEventWatcher stopWatcher;
        private readonly Settings settings;
        private readonly ObservableCollection<NotifyingDataContext> gameClients;
        private readonly ObservableCollection<NotifyingDataContext> presetGroups;
        private readonly List<int> clientProcesses;
        private readonly Dictionary<int, Timer> clientTimers;
        private Timer launcherTimer;
        private bool spawnMoreClients;

        public MainWindow()
        {
            InitializeComponent();

            if (!this.IsAdministrator())
            {
                MessageBox.Show("You need to run this tool as administrator!", "Net7 multi client unlocker - Huronimous", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }

            this.clientTimers = new Dictionary<int, Timer>();

            this.Closing += this.Cleanup;
            this.Loaded += this.OnLoadComplete;
            this.dataContext = new NotifyingDataContext();
            this.gameClients = new ObservableCollection<NotifyingDataContext>();
            this.clientProcesses = new List<int>();
            this.presetGroups = new ObservableCollection<NotifyingDataContext>();

            this.dataContext[DataPath.ClientList] = this.gameClients;
            this.dataContext[DataPath.PresetGroups] = this.presetGroups;
            this.DataContext = this.dataContext;

            this.startWatcher = new ManagementEventWatcher("Select * From Win32_ProcessStartTrace");
            this.startWatcher.EventArrived += this.NewProcessStarted;
            this.startWatcher.Start();
            this.stopWatcher = new ManagementEventWatcher("Select * From Win32_ProcessStopTrace");
            this.stopWatcher.EventArrived += this.ProccesStopped;
            this.stopWatcher.Start();

            this.settings = new Settings();
            this.settings.LoadSettings();
            foreach (var presetGroup in this.settings.Data.PresetGroups)
            {
                var presetGroupContext = presetGroup.ToContext();
                this.presetGroups.Add(presetGroupContext);
                if (presetGroup.Name == this.settings.Data.CurrentPresetGroup)
                {
                    this.dataContext[DataPath.CurrentPresetGroup] = presetGroupContext;
                }
            }

            this.SelectFirstPresetGroupIfNoneSelected();
            this.dataContext[DataPath.AutoAssignPresets] = this.settings.Data.AutoAssignPresets;
            this.dataContext[DataPath.RemoveMutexLock] = this.settings.Data.RemoveMutexLock;
            this.dataContext[DataPath.AutoAcceptTOS] = this.settings.Data.AutoAcceptTOS;
            this.dataContext[DataPath.AutoInterruptSizzle] = this.settings.Data.AutoInterruptSizzle;
        }

        private void AddClientTimer(int processId)
        {
            this.RemoveClientTimer(processId);
            this.clientTimers.Add(processId, new Timer(this.ClientTimerTick, new ClientInfo { ProcessId = processId, State = ClientState.WaitingOnTOS }, 0, 1000));
        }

        private void RemoveClientTimer(int processId)
        {
            if (!this.clientTimers.ContainsKey(processId))
            {
                return;
            }

            this.clientTimers[processId].Dispose();
            this.clientTimers.Remove(processId);
        }

        private void ClientTimerTick(object state)
        {
            var clientInfo = state as ClientInfo;
            if (clientInfo == null)
            {
                return;
            }

            if (!this.IsClientProcessStillRunning(clientInfo.ProcessId))
            {
                clientInfo.State = ClientState.Stopped;
                this.UpdateStatus("ClientTimerTick/Stopped", clientInfo.ProcessId.ToString());
                this.RemoveClientTimer(clientInfo.ProcessId);
                return;
            }

            switch (clientInfo.State)
            {
                case ClientState.WaitingOnTOS:
                    var displayingTOS = WindowOperations.IsTOSWindowDisplayed(clientInfo.ProcessId);
                    if (displayingTOS)
                    {
                        clientInfo.State = ClientState.DisplayingTOS;
                        this.UpdateStatus("ClientTimerTick/DisplayingTOS", clientInfo.ProcessId.ToString());
                        if (this.settings.Data.AutoAssignPresets)
                        {
                            this.AcceptTos(clientInfo.ProcessId);
                        }
                    }

                    return;
                case ClientState.DisplayingTOS:
                    var notDisplayingTOS = !WindowOperations.IsTOSWindowDisplayed(clientInfo.ProcessId);
                    if (notDisplayingTOS)
                    {
                        clientInfo.State = ClientState.WaitingOnMain;
                        this.UpdateStatus("ClientTimerTick/WaitingOnMain", clientInfo.ProcessId.ToString());
                    }

                    return;
                case ClientState.WaitingOnMain:
                    var windowHandle = WindowOperations.FindENBWindow(clientInfo.ProcessId);
                    if (windowHandle == IntPtr.Zero)
                    {
                        return;
                    }

                    var isVisible = WindowOperations.IswindowVisible(windowHandle);
                    if (!isVisible)
                    {
                        return;
                    }

                    clientInfo.State = ClientState.DisplayingMain;
                    this.UpdateStatus("ClientTimerTick/DisplayingMain", clientInfo.ProcessId.ToString());
                    break;
                case ClientState.DisplayingMain:
                    var threadCount = Process.GetProcessById(clientInfo.ProcessId).Threads.Count;
                    if (threadCount < 20)
                    {
                        return;
                    }

                    clientInfo.State = ClientState.ReadyForInteraction;
                    this.UpdateStatus("ClientTimerTick/ReadyForInteraction", clientInfo.ProcessId.ToString());
                    this.FillClientListInBackgroundWorker();
                    return;
                case ClientState.ReadyForInteraction:
                    if (!this.settings.Data.AutoInterruptSizzle)
                    {
                        this.CheckIfWeNeedToSpawnMoreGameClientsInBackgroundWorker();
                        this.RemoveClientTimer(clientInfo.ProcessId);
                        return;
                    }

                    clientInfo.State = ClientState.WaitingForSizzleKickoff1;
                    this.UpdateStatus("ClientTimerTick/WaitingForSizzleKickoff1", clientInfo.ProcessId.ToString());
                    return;
                case ClientState.WaitingForSizzleKickoff1:
                    if (!this.settings.Data.AutoInterruptSizzle)
                    {
                        this.RemoveClientTimer(clientInfo.ProcessId);
                        return;
                    }

                    clientInfo.State = ClientState.WaitingForSizzleKickoff2;
                    this.UpdateStatus("ClientTimerTick/WaitingForSizzleKickoff2", clientInfo.ProcessId.ToString());
                    return;

                case ClientState.WaitingForSizzleKickoff2:
                    if (!this.settings.Data.AutoInterruptSizzle)
                    {
                        this.RemoveClientTimer(clientInfo.ProcessId);
                    }

                    this.StopIntroPlay(clientInfo.ProcessId);
                    this.CheckIfWeNeedToSpawnMoreGameClientsInBackgroundWorker();
                    this.RemoveClientTimer(clientInfo.ProcessId);
                    return;
                default:
                    return;
            }
        }

        private void CheckIfWeNeedToSpawnMoreGameClientsInBackgroundWorker()
        {
            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.CheckIfWeNeedToSpawnMoreGameClients();
            };
            bgw.RunWorkerAsync();
        }

        private void CheckIfWeNeedToSpawnMoreGameClients()
        {
            if (!this.spawnMoreClients)
            {
                return;
            }

            if (this.GetClientList().Count() < this.GetSelectedPresetGroupRows()?.Count)
            {
                this.UpdateStatus("Spawning an extra game client.");
                this.SpawnNewGameClient();
            }
            else
            {
                this.UpdateStatus("Already have enough game clients running, not spawning more.");
                this.spawnMoreClients = false;
            }
        }

        private void SelectFirstPresetGroupIfNoneSelected()
        {
            if (this.presetGroups.Any() && this.dataContext[DataPath.CurrentPresetGroup] == null)
            {
                this.dataContext[DataPath.CurrentPresetGroup] = this.presetGroups.First();
            }
        }

        private bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent())).IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void OnLoadComplete(object sender, RoutedEventArgs e)
        {
            if (this.settings.Data.MainWindowInfo.Width > 0 && this.settings.Data.MainWindowInfo.Height > 0)
            {
                this.Left = this.settings.Data.MainWindowInfo.Left;
                this.Top = this.settings.Data.MainWindowInfo.Top;
                this.Height = this.settings.Data.MainWindowInfo.Height;
                this.Width = this.settings.Data.MainWindowInfo.Width;
                this.WindowState = this.settings.Data.MainWindowInfo.WindowState;
            }

            try
            {
                this.UpdateStatus("Start monitoring Earth & Beyond client launches.");
                this.GetRunningClients();
                this.FillClientList();
            }
            catch (ManagementException)
            {
                MessageBox.Show("This tool needs to be started as administrator or it wont work.");
            }
        }

        private void ProccesStopped(object sender, EventArrivedEventArgs e)
        {
            var processId = this.FilterClient(e.NewEvent);
            if (processId == null)
            {
                return;
            }

            this.UpdateStatus("Detected Earth & Beyond client exit", processId);
            this.FillClientList();
        }

        private void Cleanup(object sender, CancelEventArgs e)
        {
            this.startWatcher.Stop();
            this.stopWatcher.Stop();
            this.settings.Data.MainWindowInfo.Top = this.Top;
            this.settings.Data.MainWindowInfo.Left = this.Left;
            this.settings.Data.MainWindowInfo.Width = this.Width;
            this.settings.Data.MainWindowInfo.Height = this.Height;
            this.settings.Data.MainWindowInfo.WindowState = this.WindowState;

            this.settings.Data.PresetGroups = PresetGroups.FromContext(this.dataContext);
            this.settings.Data.CurrentPresetGroup = PresetGroup.FromContext(this.dataContext[DataPath.CurrentPresetGroup] as NotifyingDataContext).Name;
            foreach (var presetGroup in this.settings.Data.PresetGroups)
            {
                foreach (var presetGroupRow in presetGroup.Rows)
                {
                    if (presetGroupRow.Process.Id <= 0)
                    {
                        continue; 
                    } 

                    presetGroupRow.PreviousProcess.Id = presetGroupRow.Process.Id;
                    presetGroupRow.Process.Id = 0;
                }
            }

            this.settings.SaveSettings();
        }

        private void NewProcessStarted(object sender, EventArrivedEventArgs e)
        {
            var processIdText = this.FilterClient(e.NewEvent);
            if (processIdText == null)
            {
                return;
            }

            this.UpdateStatus("Detected Earth & Beyond client launch", processIdText);
            if (this.settings.Data.RemoveMutexLock)
            {
                this.CloseHandlesExternallyInBackgroundWorker(processIdText);
            }

            var processId = Convert.ToInt32(processIdText);
            this.AddClientTimer(processId);
        }

        private void StopIntroPlay(int processId)
        {
            var enbWindowHandle = WindowOperations.FindENBWindow(processId);
            WindowOperations.SetFocus(enbWindowHandle);
            WindowOperations.PostEscapeMessageToClientProcess(enbWindowHandle);
            this.UpdateStatus("Interrupted sizzle", processId.ToString());
        }

        private bool IsClientProcessStillRunning(int processId)
        {
            var clients = this.GetClientList();
            return clients.Any(c => c.Id == processId);
        }

        private bool IsLauncherProcessStillRunning(int processId)
        {
            var clients = this.GetLauncherList();
            return clients.Any(c => c.Id == processId);
        }

        private string FilterClient(ManagementBaseObject baseObject)
        {
            string processId = null;

            if ((string)baseObject["ProcessName"] == "client.exe")
            {
                processId = baseObject["ProcessId"].ToString();
            }

            return processId;
        }

        private void AcceptTos(int processId)
        {
            var enbWindowHandle = WindowOperations.FindENBWindow(processId);
            WindowOperations.SetFocus(enbWindowHandle);
            WindowOperations.AcceptTos(enbWindowHandle);
        }

        private void CloseHandlesExternallyInBackgroundWorker(string processId)
        {
            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.CloseHandlesExternally(processId);
            };
            bgw.RunWorkerAsync();
        }

        private void CloseHandlesExternally(string processId)
        {
            int exitCode;
            using (var unlockHelperProcess = new Process())
            {
                unlockHelperProcess.StartInfo = new ProcessStartInfo("Net7UnlockHelper.exe", processId) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
                unlockHelperProcess.Start();
                unlockHelperProcess.WaitForExit();
                exitCode = unlockHelperProcess.ExitCode;
            }

            switch (exitCode)
            {
                case 2:
                    this.UpdateStatus("Failed to disable multiple client lock for Earth & Beyond client", processId);
                    break;
                case 3:
                    this.UpdateStatus("Disabled multiple client lock for Earth & Beyond client", processId);
                    break;

                default:
                    this.UpdateStatus("Error calling the unlock helper.");
                    break;
            }
        }

        private void GetRunningClients()
        {
            foreach (int processId in this.GetClientList().Select(clientProcess => clientProcess.Id))
            {
                var id = processId.ToString(CultureInfo.InvariantCulture);
                this.UpdateStatus("Detected Earth & Beyond client already running", id);
                if (this.settings.Data.RemoveMutexLock)
                {
                    this.CloseHandlesExternallyInBackgroundWorker(id);
                }
            }
        }

        private IEnumerable<Process> GetClientList()
        {
            return Process.GetProcessesByName("client");
        }

        private IEnumerable<Process> GetLauncherList()
        {
            return Process.GetProcessesByName("LAUNCHNET7");
        }

        private void SafeInvoke(Action action)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }

        private void FillClientListInBackgroundWorker()
        {
            var bgw = new BackgroundWorker();
            bgw.DoWork += (_, __) =>
            {
                this.FillClientList();
            };
            bgw.RunWorkerAsync();
        }

        private void FillClientList()
        {
            this.SafeInvoke(() =>
            {
                // Get all current running client processes
                var clientList = this.GetClientList().ToList();

                // Remove all no-longer-running clients from the clientProcess list and the gameClients list
                var clientsThatStoppedRunningSinceLastTime = this.clientProcesses.Where(cp => clientList.All(cl => cl.Id != cp)).ToList();
                foreach (var clientProcess in clientsThatStoppedRunningSinceLastTime)
                {
                    clientProcesses.Remove(clientProcess);
                    var gameClient = gameClients.FirstOrDefault(g => Convert.ToInt32(g["Id"]) == clientProcess);
                    if (gameClient != null)
                    {
                        gameClients.Remove(gameClient);
                    }
                }

                // Add all new running clients to the clientProcess list and the gameClients list.
                var newClientsThatStartedRunningSinceLastTime = clientList.Where(cl => this.clientProcesses.All(cp => cl.Id != cp)).ToList();
                foreach (var process in newClientsThatStartedRunningSinceLastTime)
                {
                    clientProcesses.Add(process.Id);
                    gameClients.Add(this.CreateProcessContext(process.Id));
                }

                // Get the currently selected preset and it's rows.
                var currentPresetGroup = this.dataContext["CurrentPresetGroup"] as NotifyingDataContext;
                var rows = currentPresetGroup?["Rows"] as ObservableCollection<NotifyingDataContext>;
                if (rows == null)
                {
                    return;
                }

                // Go over all rows and see if we can read the process.
                // If we find the process, we try to read the current id
                // If the current id is no longer in the list of running processes
                // We set the PreviousProcess.Id to the current process id
                // And we set the Process.Id to 0 (removes the selected process from the row)
                var assignedProcessIds = new List<int>();

                // Save all rows that are unassigned to use for auto assign.
                var availableRows = new Dictionary<int, NotifyingDataContext>();
                var zeroProcessIdReplacement = -1;

                foreach (var context in rows)
                {
                    var processId = 0;
                    var processContext = context["Process"] as NotifyingDataContext;
                    if (processContext != null)
                    {
                        processId = this.ParseInt(processContext["Id"]);
                    }

                    var id = processId;
                    if (clientList.Any(c => c.Id == id))
                    {
                        assignedProcessIds.Add(processId);
                        continue;
                    }

                    context["PreviousProcess"] = this.CreateProcessContext(processId);

                    var combobox = context["Combobox"] as ComboBox;
                    if (combobox != null)
                    {
                        combobox.SelectedItem = this.CreateProcessContext(0);
                    }

                    if (processId == 0)
                    {
                        processId = zeroProcessIdReplacement;
                        zeroProcessIdReplacement--;
                    }

                    availableRows.Add(processId, context);
                }

                if (!this.settings.Data.AutoAssignPresets || availableRows.Count == 0)
                {
                    return;
                }

                // Get a list of all client process ids that are not assigned to any row.
                var unassignedClients = this.clientProcesses.Where(c => !assignedProcessIds.Contains(c)).ToList();

                // Make a list of available rows that previously were assigned to a process id that is now running and unassigned
                // This way we can 'discover' the same row for this client that it had before the tool stopped.
                var formerClients = availableRows.Keys.Where(k => unassignedClients.Contains(k)).ToList();
                foreach (var formerClient in formerClients)
                {
                    var combobox = availableRows[formerClient]["Combobox"] as ComboBox;
                    if (combobox != null)
                    {
                        var gameClient = gameClients.FirstOrDefault(g => Convert.ToInt32(g["Id"]) == formerClient);
                        if (gameClient != null)
                        {
                            combobox.SelectedItem = gameClient;
                        }
                    }

                    unassignedClients.Remove(formerClient);
                    availableRows.Remove(formerClient);
                }

                // Go through all remaining available rows. Once there are no unsassigned clients anymore, we stop.
                // For each unassigned client we still have, we assign it to the first row and continue to the next row.
                foreach (var context in availableRows.Values)
                {
                    if (!unassignedClients.Any())
                    {
                        return;
                    }

                    var client = unassignedClients.First();
                    unassignedClients.Remove(client);

                    var combobox = context["Combobox"] as ComboBox;
                    if (combobox != null)
                    {
                        var gameClient = gameClients.FirstOrDefault(g => Convert.ToInt32(g["Id"]) == client);
                        if (gameClient != null)
                        {
                            combobox.SelectedItem = gameClient;
                        }
                    }

                    // context["Process"] = null;
                    // context["Process"] = this.CreateProcessContext(client);
                }
            });
        }

        private void SetClientLocation(NotifyingDataContext presetGroupRowContext)
        {
            if (presetGroupRowContext == null)
            {
                return;
            }

            var process = this.GetProcessSafe(presetGroupRowContext);

            if (process == null)
            {
                return;
            }

            var left = this.ParseInt(presetGroupRowContext["Left"]);
            var top = this.ParseInt(presetGroupRowContext["Top"]);

            var processMainWindowHandle = WindowOperations.FindENBWindow(process.Id);
            WindowOperations.MoveClientWindow(processMainWindowHandle, left, top);
        }

        private void SetMutualExclusiveClient(NotifyingDataContext notifyingDataContext)
        {
            var currentProcess = notifyingDataContext?["Process"] as NotifyingDataContext;
            if (currentProcess == null)
            {
                return;
            }

            var currentPresetGroup = this.dataContext["CurrentPresetGroup"] as NotifyingDataContext;
            var rows = currentPresetGroup?["Rows"] as ObservableCollection<NotifyingDataContext>;
            if (rows == null)
            {
                return;
            }

            foreach (var context in rows.Where(r => r["Process"] == currentProcess && r != notifyingDataContext))
            {
                context["Process"] = null;
                context["Process"] = this.CreateProcessContext(0);
            }
        }

        private void UpdateStatus(string status, string processId)
        {
            var statusText = $"{DateTime.Now} - {status} ({processId}){Environment.NewLine}";
            this.SafeInvoke(() => this.SafeUpdateStatus(statusText));
        }

        private void UpdateStatus(string status)
        {
            var statusText = $"{DateTime.Now} - {status}{Environment.NewLine}";
            this.SafeInvoke(() => this.SafeUpdateStatus(statusText));
        }

        private void SafeUpdateStatus(string statusText)
        {
            StatusTextBox.AppendText(statusText);
            StatusTextBox.ScrollToEnd();
        }

        private Process GetProcessSafe(NotifyingDataContext presetGroupRowContext)
        {
            var processContext = presetGroupRowContext?["Process"] as NotifyingDataContext;

            if (processContext == null)
            {
                return null;
            }

            int id = this.ParseInt(processContext["Id"]);
            Process process = null;
            try
            {
                process = Process.GetProcessById(id);
            }
            catch (ArgumentException)
            {
            }

            return process;
        }

        private void SetLocation(NotifyingDataContext presetGroupRowContext, int wait = 0)
        {
            if (presetGroupRowContext == null)
            {
                return;
            }

            if (wait > 0)
            {
                var bgw = new BackgroundWorker();
                bgw.DoWork += (_, __) =>
                    {
                        Thread.Sleep(wait);
                        this.SetLocation(presetGroupRowContext);
                    };  
                bgw.RunWorkerAsync();
                return;
            }

            this.SetClientLocation(presetGroupRowContext);
        }

        private void KillGameClient(Process process)
        {
            process.Kill();
        }

        private void FlashClient(Process process)
        {
            if (process == null)
            {
                return;
            }

            WindowOperations.FlashWindowEx(WindowOperations.FindENBWindow(process.Id), 3);
        }

        private void SetWindowText(NotifyingDataContext presetGroupRowContext)
        {
            if (presetGroupRowContext == null)
            {
                return;
            }

            var accountName = this.ParseString(presetGroupRowContext["AccountName"]);
            if (String.IsNullOrWhiteSpace(accountName))
            {
                return;
            }

            var process = this.GetProcessSafe(presetGroupRowContext);

            if (process == null)
            {
                return;
            }

            WindowOperations.SetWindowText(WindowOperations.FindENBWindow(process.Id), $"Earth & Beyond - {accountName}");
        }

        private void ClickHandler(object sender, Action<NotifyingDataContext> action)
        {
            var presetGroupRowContext = this.GetTagObject<NotifyingDataContext>(sender);
            if (presetGroupRowContext == null)
            {
                return;
            }

            action.Invoke(presetGroupRowContext);
        }

        private void ClickHandler(object sender, Action<NotifyingDataContext>[] actions)
        {
            var presetGroupRowContext = this.GetTagObject<NotifyingDataContext>(sender);
            if (presetGroupRowContext == null)
            {
                return;
            }

            foreach (var action in actions)
            {
                action.Invoke(presetGroupRowContext);
            }
        }

        private void ClickHandler(object sender, Action<Process> action)
        {
            var presetGroupRowContext = this.GetTagObject<NotifyingDataContext>(sender);
            var value = presetGroupRowContext?["Process"] as NotifyingDataContext;

            if (value == null)
            {
                return;
            }

            var process = this.GetProcessSafe(presetGroupRowContext);
            if (process == null)
            {
                return;
            }

            action.Invoke(process);
        }

        private T GetTagObject<T>(object sender) where T : class
        {
            var frameworkElement = sender as FrameworkElement;
            return frameworkElement?.Tag as T;
        }

        private void SetLocationButtonClick(object sender, RoutedEventArgs e)
        {
            this.ClickHandler(sender, p => this.SetLocation(p));
        }

        private void KillGameClientClick(object sender, RoutedEventArgs e)
        {
            this.ClickHandler(sender, this.KillGameClient);
        }

        private void OnSelectedClientPresetChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ClickHandler(sender,  new Action<NotifyingDataContext>[] { this.SetClientLocation, this.SetMutualExclusiveClient, this.SetWindowText });
        }

        private void FlashGameClientClick(object sender, RoutedEventArgs e)
        {
            this.ClickHandler(sender, this.FlashClient);
        }

        private void AddPresetGroupClick(object sender, RoutedEventArgs e)
        {
            var input = new Input(this, "Enter a name for the new preset.", String.Empty);
            input.ShowDialog();
            if (input.Canceled || String.IsNullOrWhiteSpace(input.InputText))
            {
                return;
            }

            if (this.presetGroups.Any(s => s["Name"].ToString().Equals(input.InputText, StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageBox.Show($"Cannot add Preset because a preset group with name {input.InputText} already exists.");
                return;
            }

            var presetGroup = this.CreatePresetGroup(input.InputText);
            this.presetGroups.Add(presetGroup);
            this.dataContext[DataPath.CurrentPresetGroup] = presetGroup;
        }

        private void RemovePresetGroupClick(object sender, RoutedEventArgs e)
        {
            var currentPresetGroup = this.GetCurrentPresetGroup();
            if (currentPresetGroup == null)
            {
                return;
            }

            var result = MessageBox.Show($"Are you sure you want to delete the preset group with name [{currentPresetGroup["Name"]}]?", "Delete preset group confirmation", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                this.presetGroups.Remove(currentPresetGroup);
            }

            this.SelectFirstPresetGroupIfNoneSelected();
        }

        private NotifyingDataContext GetCurrentPresetGroup()
        {
            return this.dataContext[DataPath.CurrentPresetGroup] as NotifyingDataContext;
        }

        private ObservableCollection<NotifyingDataContext> GetSelectedPresetGroupRows()
        {
            return this.GetCurrentPresetGroup()?["Rows"] as ObservableCollection<NotifyingDataContext>;
        }

        private void AddPresetGroupRow(object sender, RoutedEventArgs e)
        {
            var currentPresetGroupRows = this.GetSelectedPresetGroupRows();
            currentPresetGroupRows?.Add(this.CreatePresetGroupRow());
        }

        private void RemovePresetGroupRow(object sender, RoutedEventArgs e)
        {
            var currentPresetGroupRows = this.GetSelectedPresetGroupRows();
            var tagObject = this.GetTagObject<NotifyingDataContext>(sender);
            if (tagObject == null)
            {
                return;
            }

            currentPresetGroupRows?.Remove(tagObject);
        }

        private void NudgeLeft(object sender, RoutedEventArgs e)
        {
            var tagObject = this.GetTagObject<NotifyingDataContext>(sender);
            tagObject["Left"] = this.ParseInt(tagObject["Left"]) - 1;
            this.SetClientLocation(tagObject);
        }

        private void NudgeRight(object sender, RoutedEventArgs e)
        {
            var tagObject = this.GetTagObject<NotifyingDataContext>(sender);
            tagObject["Left"] = this.ParseInt(tagObject["Left"]) + 1;
            this.SetClientLocation(tagObject);
        }

        private void NudgeUp(object sender, RoutedEventArgs e)
        {
            var tagObject = this.GetTagObject<NotifyingDataContext>(sender);
            tagObject["Top"] = this.ParseInt(tagObject["Top"]) - 1;
            this.SetClientLocation(tagObject);
        }

        private void NudgeDown(object sender, RoutedEventArgs e)
        {
            var tagObject = this.GetTagObject<NotifyingDataContext>(sender);
            tagObject["Top"] = this.ParseInt(tagObject["Top"]) + 1;
            this.SetClientLocation(tagObject);
        }

        private NotifyingDataContext CreatePresetGroup(string name)
        {
            return new NotifyingDataContext
            {
                ["Name"] = name,
                ["Rows"] = new ObservableCollection<NotifyingDataContext>()
            };
        }

        private NotifyingDataContext CreatePresetGroupRow()
        {
            return new NotifyingDataContext(new Dictionary<DataPath, object>())
            {
                ["PreviousProcess"] = this.CreateProcessContext(0),
                ["Process"] = this.CreateProcessContext(0),
                ["Left"] = 0,
                ["Top"] = 0,
                ["AccountName"] = String.Empty,
                ["Password"] = String.Empty
            };
        }

        private NotifyingDataContext CreateProcessContext(int id)
        {
            return new NotifyingDataContext(new Dictionary<DataPath, object>()) { ["Id"] = id };
        }

        private int ParseInt(object value)
        {
            return Convert.ToInt32(value);
        }

        private string ParseString(object value)
        {
            return Convert.ToString(value);
        }

        private void LeftChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            var tagObject = this.GetTagObject<NotifyingDataContext>(sender);
            this.SetClientLocation(tagObject);
        }

        private void TopChanged(object sender, RoutedEventArgs routedEventArgs)
        {
            var tagObject = this.GetTagObject<NotifyingDataContext>(sender);
            this.SetClientLocation(tagObject);
        }

        private void AccountNameChanged(object sender, RoutedEventArgs e)
        {
            this.ClickHandler(sender, this.SetWindowText);
        }

        private void PasswordChanged(object sender, RoutedEventArgs e)
        {
        }

        private void RowInitialized(object sender, EventArgs e)
        {
            var frameworkElement = sender as ComboBox;
            var context = frameworkElement?.Tag as NotifyingDataContext;

            if (context == null)
            {
                return;
            }

            context["Combobox"] = frameworkElement;
        }

        private void RemoveMultiClientLockCheckClick(object sender, RoutedEventArgs e)
        {
            this.settings.Data.RemoveMutexLock = !this.settings.Data.RemoveMutexLock;
        }

        private void AutoAssignPresetsCheckClick(object sender, RoutedEventArgs e)
        {
            this.settings.Data.AutoAssignPresets = !this.settings.Data.AutoAssignPresets;
        }

        private void AutoAcceptTOSCheckClicked(object sender, RoutedEventArgs e)
        {
            this.settings.Data.AutoAcceptTOS = !this.settings.Data.AutoAcceptTOS;
        }

        private void AutoInterruptSizzleCheckClicked(object sender, RoutedEventArgs e)
        {
            this.settings.Data.AutoInterruptSizzle = !this.settings.Data.AutoInterruptSizzle;
        }

        private void StartAllClientsClick(object sender, RoutedEventArgs e)
        {
            this.spawnMoreClients = true;
            this.CheckIfWeNeedToSpawnMoreGameClientsInBackgroundWorker();
        }

        private void SpawnNewGameClient()
        {
            if (String.IsNullOrWhiteSpace(this.settings.Data.PathToNet7Launcher))
            {
                var openFileDialog = new OpenFileDialog
                {
                    FileName = "LaunchNet7.exe",
                    AddExtension = true
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    this.settings.Data.PathToNet7Launcher = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            if (String.IsNullOrWhiteSpace(this.settings.Data.PathToNet7Launcher))
            {
                this.SpawnNewGameClient();
                return;
            }

            if (!System.IO.File.Exists(this.settings.Data.PathToNet7Launcher))
            {
                this.SpawnNewGameClient();
                return;
            }

            var folder = System.IO.Path.GetDirectoryName(this.settings.Data.PathToNet7Launcher);
            if (String.IsNullOrWhiteSpace(folder))
            {
                this.SpawnNewGameClient();
                return;
            }

            using (var launcherProcess = new Process
            {
                StartInfo = new ProcessStartInfo(this.settings.Data.PathToNet7Launcher)
                {
                    WorkingDirectory = folder,
                    UseShellExecute = true,
                    LoadUserProfile = true
                }
            })
            {
                launcherProcess.Start();
                this.launcherTimer = new Timer(this.LauncherTimerTick, new LauncherInfo { ProcessId = launcherProcess.Id, State = LauncherState.WaitingOnPlayButton }, 0, 1000);
            }
        }

        private void LauncherTimerTick(object state)
        {
            var launcherInfo = state as LauncherInfo;
            if (launcherInfo == null)
            {
                return;
            }

            if (!this.IsLauncherProcessStillRunning(launcherInfo.ProcessId))
            {
                launcherInfo.State = LauncherState.Stopped;
                this.UpdateStatus("LauncherTimerTick/Stopped", launcherInfo.ProcessId.ToString());
                this.launcherTimer.Dispose();
                return;
            }

            switch (launcherInfo.State)
            {
                case LauncherState.WaitingOnPlayButton:
                    var isPlayButtonDisplayed = WindowOperations.IsLaucherPlayButtonDisplayed(Process.GetProcessById(launcherInfo.ProcessId).MainWindowHandle);
                    if (isPlayButtonDisplayed)
                    {
                        launcherInfo.State = LauncherState.DisplayingPlayButton;
                        this.UpdateStatus("LauncherTimerTick/DisplayingPlayButton", launcherInfo.ProcessId.ToString());
                    }

                    return;
                case LauncherState.DisplayingPlayButton:
                    launcherInfo.State = LauncherState.ClickedPlayButton;
                    var launcherWindowHandle = Process.GetProcessById(launcherInfo.ProcessId).MainWindowHandle;
                    WindowOperations.SetFocus(launcherWindowHandle);
                    WindowOperations.LauncherPlayButton(launcherWindowHandle);
                    break;
            }
        }

        private void KillAllClientsClick(object sender, RoutedEventArgs e)
        {
            var clients = this.GetClientList().ToList();
            foreach (var process in clients)
            {
                process.Kill();
            }
        }
    }
}
