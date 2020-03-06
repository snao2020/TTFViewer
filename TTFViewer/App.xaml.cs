using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TTFViewer.Model;
using TTFViewer.View;
using TTFViewer.ViewModel;

namespace TTFViewer
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            TTFModel model = new TTFModel();
            TTFViewModel viewModel = new TTFViewModel(model);
            TTFView view = new TTFView() { DataContext = viewModel };
            view.Show();
        }
    }
}
