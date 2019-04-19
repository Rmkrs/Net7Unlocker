namespace Net7MultiClientUnlocker.Domain
{
    using System.Collections.Generic;

    public class DefaultResolutions
    {
        public IEnumerable<Resolution> Create()
        {
            var resolutions = new List<Resolution>
                                  {
                                      new Resolution { Width = 800, Height = 600 },
                                      new Resolution { Width = 1024, Height = 768 },
                                      new Resolution { Width = 1152, Height = 864 },
                                      new Resolution { Width = 1280, Height = 720 },
                                      new Resolution { Width = 1280, Height = 768 },
                                      new Resolution { Width = 1280, Height = 800 },
                                      new Resolution { Width = 1280, Height = 960 },
                                      new Resolution { Width = 1280, Height = 1024 },
                                      new Resolution { Width = 1280, Height = 768 },
                                      new Resolution { Width = 1280, Height = 768 },
                                      new Resolution { Width = 1280, Height = 900 },
                                      new Resolution { Width = 1360, Height = 900 },
                                      new Resolution { Width = 1366, Height = 1024 },
                                      new Resolution { Width = 1440, Height = 1200 },
                                      new Resolution { Width = 1600, Height = 900 },
                                      new Resolution { Width = 1600, Height = 1024 },
                                      new Resolution { Width = 1600, Height = 1200 }
                                  };

            return resolutions;
        }
    }
}
