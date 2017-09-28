using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageMaker
{
    class ImageControllerSingleton:ImageController
    {
        private static ImageControllerSingleton instance;

        static ImageControllerSingleton()
        { }

        public static ImageControllerSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ImageControllerSingleton();
                }
                return instance;
            }
        }
    }
}
