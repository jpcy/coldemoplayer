using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;

namespace CDP
{
    public class IconCache
    {
        private readonly Core.ISettings settings = Core.ObjectCreator.Get<Core.ISettings>();
        private readonly Core.IFileSystem fileSystem = Core.ObjectCreator.Get<Core.IFileSystem>();
        private readonly List<BitmapImage> cache = new List<BitmapImage>();
        private readonly BitmapImage unknown;

        public IconCache()
        {
            unknown = new BitmapImage(new Uri(fileSystem.PathCombine(settings.ProgramPath, "icons", "unknown.ico")));
            unknown.Freeze();
            cache.Add(unknown);
        }

        /// <summary>
        /// Find the first icon that can be loaded from the given list of possible icon locations. If no icon is found, an "unknown" icon is returned.
        /// </summary>
        /// <param name="fileNames">The possible icon locations.</param>
        /// <returns>An icon as a BitmapImage.</returns>
        public BitmapImage FindIcon(IEnumerable<string> fileNames)
        {
            if (fileNames == null)
            {
                throw new ArgumentNullException("fileNames");
            }

            foreach (string fileName in fileNames)
            {
                BitmapImage icon = LoadIcon(fileName);

                if (icon != null)
                {
                    return icon;
                }
            }

            return unknown;
        }

        /// <summary>
        /// Load an icon from disk, or from the cache if the icon has already been loaded.
        /// </summary>
        /// <param name="fileName">The filename of the icon.</param>
        /// <returns>An icon as a BitmapImage, or null if the icon cannot be loaded.</returns>
        public BitmapImage LoadIcon(string fileName)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (!File.Exists(fileName))
            {
                return null;
            }

            BitmapImage icon = cache.FirstOrDefault(bi => bi.UriSource.Equals(new Uri(fileName)));

            if (icon == null)
            {
                icon = new BitmapImage(new Uri(fileName));
                icon.Freeze();
                cache.Add(icon);
            }

            return icon;
        }
    }
}
