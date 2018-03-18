
namespace Spike.Instrumentation.Logging
{
    using System;

    public class LogFactory
    {
        public static Logging Create(string module = null, bool disableConsoleLogging = false)
        {
            return new Logging(module, disableConsoleLogging);
        }

        public static T CreateSpecialized<T>(string module = null, bool disableConsoleLogging = false) where T : Logging
        {
            try
            {
                var type = typeof (T);

                if (module == null)
                {
                    return Activator.CreateInstance(type) as T;
                }
                return Activator.CreateInstance(type, module, disableConsoleLogging) as T;
            }
            catch {return null; }
        }
    }
}
