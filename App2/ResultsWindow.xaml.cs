using System.Collections.Generic;
using System.Windows;

namespace App2
{
    //Opens and new window and populates the DataGrid based on ExerciseObj.cs
    public partial class ResultsWindow : Window
    {
        public ResultsWindow(List<PassingObject> content)
        {
            InitializeComponent();
            dataGrid.ItemsSource = content;
            DataContext = this;
        }
    }
}
