using LBManager.Infrastructure.Interfaces;
using LBManager.Infrastructure.Models;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    public class ScreenViewModel:BindableBase
    {
        public ScreenViewModel(Screen screen)
        {
            _id = screen.Id;
            _name = screen.Name;
            _width = screen.Width;
            _height = screen.Height;
            _pixelsWidth = screen.PixelsWidth;
            _pixelsHeight = screen.PixelsHeight;
        }

        private string _id;
        public string Id
        {
            get { return _id; }
            set { SetProperty(ref _id, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private int _width;
        public int Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private int _height;
        public int Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        private int _pixelsWidth;
        public int PixelsWidth
        {
            get { return _pixelsWidth; }
            set { SetProperty(ref _pixelsWidth, value); }
        }

        private int _pixelsHeight;
        public int PixelsHeight
        {
            get { return _pixelsHeight; }
            set { SetProperty(ref _pixelsHeight, value); }
        }
    }
}
