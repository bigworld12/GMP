using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMP.Extentions
{
    /// <summary>
    /// Represents an Object that can be assigned a value and represeneted with another value
    /// </summary>
    /// <typeparam name="TFrom">the base type that will be represented</typeparam>
    /// <typeparam name="TTo">the representation type</typeparam>\
    /// <remarks>It implments the INotifyPropertyChanged interface, so it supports binding</remarks>
    public class ReadableObject<TFrom, TTo> : INotifyPropertyChanged where TTo : class
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OPC(string propname)
        {
            PropertyChanged?.Invoke(this , new PropertyChangedEventArgs(propname));
        }


        public ReadableObject()
        {
            if (typeof(TTo) == typeof(string))
            {
                RepresentationFunction = (x) => { return x.ToString() as TTo; };
            }
        }
        public ReadableObject(RepresentationFormula CalculationFormula)
        {
            RepresentationFunction = CalculationFormula;
        }

        public delegate TTo RepresentationFormula(TFrom ToRepresent);

        private TFrom m_BaseValue;
        public TFrom BaseValue
        {
            get { return m_BaseValue; }
            set
            {
                if (m_BaseValue != null && m_BaseValue is INotifyPropertyChanged)
                {
                    (m_BaseValue as INotifyPropertyChanged).PropertyChanged -= BaseValue_PropertyChanged;
                }
                if (value != null && value is INotifyPropertyChanged)
                {
                    (value as INotifyPropertyChanged).PropertyChanged += BaseValue_PropertyChanged;
                }
                m_BaseValue = value;
                OPC(nameof(BaseValue));
                OPC(nameof(RepresentationValue));
            }
        }

        private void BaseValue_PropertyChanged(object sender , PropertyChangedEventArgs e)
        {
            OPC(nameof(RepresentationValue));
        }

        
        public TTo RepresentationValue
        {
            get
            {
                if (RepresentationFunction != null)
                {
                    return RepresentationFunction(BaseValue);
                }
                else
                {
                    if (typeof(TTo) == typeof(string)) return BaseValue.ToString() as TTo;
                    else if (typeof(TFrom) == typeof(TTo))
                    {
                        return BaseValue as TTo;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        private RepresentationFormula m_repfor;
        public RepresentationFormula RepresentationFunction
        {
            get { return m_repfor; }
            set
            {
                m_repfor = value;
                OPC(nameof(RepresentationFunction));
                OPC(nameof(RepresentationValue));
            }
        }

    }
}
