namespace Plainion.DrawVista.IO
{
    public class DrawIOAppFactory
    {

        private static DrawIOAppFactory _instance;

        private static readonly object _lock = new object();

        private DrawIOAppFactory()
        {
        }

        public static DrawIOAppFactory Instance
        {
            get
            {
                if (_instance != null) return _instance;
                lock (_lock)
                {
                    _instance ??= new DrawIOAppFactory();
                }

                return _instance;
            }
        }

        public IDrawIOApp CreateDrawIOAppExtension(DrawIOModel model)
        {
            return new DrawIOAppExtension(model);
        }

        public IDrawIOApp CreateDrawIOApp(DrawIOModel model)
        {
            return new DrawIOApp(model);
        }
    }
}
