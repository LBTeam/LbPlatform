using LBManager.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LBManager.Infrastructure.Models
{
    public class Media
    {
        private IChecksumProvider _checksumProvider;
        public Media()
        {
        }


        private string _name = string.Empty;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _id = string.Empty;

        public string Id
        {
            get { return _id; }
            private set
            {
                _id = value;
            }
        }



    }
}
