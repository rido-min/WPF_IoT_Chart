using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace WPF_IoT_Chart
{
    public class ViewModel
    {

        private int _index = 0;
        private readonly Random _random = new Random();
        private readonly ObservableCollection<ObservablePoint> _observableValues;

        public ViewModel()
        {
            _observableValues = new ObservableCollection<ObservablePoint>
            {
                // using an object that implements INotifyPropertyChanged
                // will allow the chart to update everytime a property in a point changes.

                // LiveCharts already provides the ObservableValue class
                // notice you can plot any type, but you must let LiveCharts know how to handle it
                // for more info please see:
                // https://github.com/beto-rodriguez/LiveCharts2/blob/master/samples/ViewModelsSamples/General/UserDefinedTypes/ViewModel.cs#L22

                new ObservablePoint(_index++, 0),
                new ObservablePoint(_index++, 0),
                new ObservablePoint(_index++, 0),
                new ObservablePoint(_index++, 0),
                new ObservablePoint(_index++, 0)
            };

            Series = new ObservableCollection<ISeries>
            {
                new LineSeries<ObservablePoint>
                {
                    Values = _observableValues,
                    Fill = null
                }
            };
        }

        public ObservableCollection<ISeries> Series { get; set; }

        public void AddItem(double t)
        {
            if (_observableValues.Count > 10)
            {
                RemoveFirstItem();
            }
            _observableValues.Add(new ObservablePoint { X = _index++, Y = t });
        }

        public void RemoveFirstItem()
        {
            if (_observableValues.Count == 0) return;
            _observableValues.RemoveAt(0);
        }

    }
}
