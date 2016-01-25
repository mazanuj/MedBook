using System.ComponentModel;
using System.Runtime.CompilerServices;
using MedBookLib.Annotations;

namespace MedBookLib.DataTypes
{
    public class InfoStruct : INotifyPropertyChanged
    {
        private string armVlast;
        private string okazannya;
        private string astosuvannya;
        private string rotupokazannya;
        private string berigannya;

        public string FarmVlast
        {
            get { return armVlast; }
            set
            {
                armVlast = value;
                OnPropertyChanged();
            }
        }

        public string Pokazannya
        {
            get { return okazannya; }
            set
            {
                okazannya = value;
                OnPropertyChanged();
            }
        }

        public string Zastosuvannya
        {
            get { return astosuvannya; }
            set
            {
                astosuvannya = value;
                OnPropertyChanged();
            }
        }

        public string Protupokazannya
        {
            get { return rotupokazannya; }
            set
            {
                rotupokazannya = value;
                OnPropertyChanged();
            }
        }

        public string Zberigannya
        {
            get { return berigannya; }
            set
            {
                berigannya = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}