using GuiMisc;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;

namespace TTFViewer.View
{
    public class ErrorMessageAction : TriggerAction<Window>
    {
        protected override void Invoke(object parameter)
        {
            if (parameter is ErrorEventArgs e)
            {
                Exception exception = e.GetException();
                if (exception != null)
                    MessageBox.Show(exception.Message);
            }
        }
    }


    public class TTFViewBehavior : Behavior<Window>
    {
        public static DependencyProperty OpenVMCommandProperty =
            DependencyProperty.Register("OpenVMCommand", typeof(ICommand), typeof(TTFViewBehavior), null);

        public ICommand OpenVMCommand
        {
            get { return (ICommand)GetValue(OpenVMCommandProperty); }
            set { SetValue(OpenVMCommandProperty, value); }
        }


        public static DependencyProperty CloseVMCommandProperty =
            DependencyProperty.Register("CloseVMCommand", typeof(ICommand), typeof(TTFViewBehavior), null);

        public ICommand CloseVMCommand
        {
            get { return (ICommand)GetValue(CloseVMCommandProperty); }
            set { SetValue(CloseVMCommandProperty, value); }
        }


        public ICommand ExitCommand { private get; set; }


        CommandBinding OpenCommandBinding;
        CommandBinding CloseCommandBinding;
        CommandBinding ExitCommandBinding;

        RoutedEventHandler TreeViewItem_SelectedHandler; 

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SourceInitialized += OnSourceInitialized;
            AssociatedObject.Closing += OnClosing;
            AssociatedObject.Closed += OnClosed;
            if (TreeViewItem_SelectedHandler == null)
                TreeViewItem_SelectedHandler = new RoutedEventHandler(TreeViewItem_Selected);
            AssociatedObject.AddHandler(TreeViewItem.SelectedEvent, TreeViewItem_SelectedHandler);
            AddCommandBindings();
        }

        void func()
        {
            if (AssociatedObject.FindName("TreeView") is TreeView tv)
            {
                Debug.WriteLine("tv_TargetUpdated {0}", tv.Items.Count);
                if (tv.Items.Count > 0)
                {
                    Debug.WriteLine("--{0} {1}",
                        tv.Items[0],
                        tv.ItemContainerGenerator.ContainerFromIndex(0));
                    if (tv.ItemContainerGenerator.ContainerFromIndex(0) is TreeViewItem tvi)
                        tvi.IsSelected = true;
                }
            }
        }

        private void Tv_TargetUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            AssociatedObject.Dispatcher.BeginInvoke(new Action(func), DispatcherPriority.Background, null);
        }

        protected override void OnDetaching()
        {
            RemoveCommandBindings();

            AssociatedObject.RemoveHandler(TreeViewItem.SelectedEvent, TreeViewItem_SelectedHandler);
            AssociatedObject.SourceInitialized -= OnSourceInitialized;
            AssociatedObject.Closing -= OnClosing;
            AssociatedObject.Closed -= OnClosed;

            base.OnDetaching();
        }


        void OnSourceInitialized(object sender, EventArgs e)
        {
            var wp = Properties.Settings.Default.WindowPlacement;
            AssociatedObject.SetWindowPlacement(wp);
            Properties.Settings.Default.WindowPlacement = null;
            if (AssociatedObject.FindName("TreeView") is TreeView tv)
            {
                tv.TargetUpdated += Tv_TargetUpdated;
            }
        }


        void OnClosing(object sender, CancelEventArgs e)
        {
            ICommand command = CloseVMCommand;
            if (command != null && command.CanExecute(null))
                command.Execute(null);

            var wp = AssociatedObject.GetWindowPlacement();
            Properties.Settings.Default.WindowPlacement = wp;
        }


        void OnClosed(object sender, EventArgs e)
        {
            if (AssociatedObject.FindName("TreeView") is TreeView tv)
            {
                tv.TargetUpdated -= Tv_TargetUpdated;
            }
            Properties.Settings.Default.Save();
        }


        static TreeView GetTreeView(TreeViewItem tvi)
        {
            TreeView result = null;
            for (DependencyObject dep = tvi; result == null && dep != null; dep = VisualTreeHelper.GetParent(dep))
            {
                if (dep is TreeView tv)
                    result = tv;
            }
            return result;
        }


        void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem tvi)
            {
                for (var parent = VisualTreeHelper.GetParent(tvi); parent != null; parent = VisualTreeHelper.GetParent(parent))
                {
                    if (parent is TreeViewItem parentTVI)
                        parentTVI.IsExpanded = true;
                }

                DependencyObject scope = FocusManager.GetFocusScope(tvi);
                if (FocusManager.GetFocusedElement(scope) is TreeViewItem focused)
                {
                    TreeView curr = GetTreeView(focused);
                    if (curr == null || curr == GetTreeView(tvi))
                    {
                        FocusManager.SetFocusedElement(scope, tvi);
                    }
                }
            }
        }


        void AddCommandBindings()
        {
            if (OpenCommandBinding == null)
                OpenCommandBinding = new CommandBinding(ApplicationCommands.Open, Open_Executed, Open_CanExecute);
            AssociatedObject.CommandBindings.Add(OpenCommandBinding);

            if (CloseCommandBinding == null)
                CloseCommandBinding = new CommandBinding(ApplicationCommands.Close, Close_Executed, Close_CanExecute);
            AssociatedObject.CommandBindings.Add(CloseCommandBinding);
            if (CloseVMCommand != null)
                CloseVMCommand.CanExecuteChanged += Close_CanExecuteChanged;

            if (ExitCommand != null)
            {
                if (ExitCommandBinding == null)
                    ExitCommandBinding = new CommandBinding(ExitCommand, Exit_Executed);
                AssociatedObject.CommandBindings.Add(ExitCommandBinding);
            }
        }


        void RemoveCommandBindings()
        {
            if (ExitCommandBinding != null)
                AssociatedObject.CommandBindings.Remove(ExitCommandBinding);

            if (CloseVMCommand != null)
                CloseVMCommand.CanExecuteChanged -= Close_CanExecuteChanged;
            if (CloseCommandBinding != null)
                AssociatedObject.CommandBindings.Remove(CloseCommandBinding);

            if (OpenCommandBinding != null)
                AssociatedObject.CommandBindings.Remove(OpenCommandBinding);
        }


        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = OpenVMCommand != null && OpenVMCommand.CanExecute(null);
        }


        void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (OpenVMCommand != null && OpenVMCommand.CanExecute(null))
            {
                FileDialog fd = new FileDialog
                {
                    Owner = AssociatedObject
                };

                if (fd.ShowDialog() == true)
                {
                    if (fd.FileInfo != null)
                    {
                        OpenVMCommand.Execute(fd.FileInfo.FullName);
                    }
                }
            }
        }


        private void Close_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = CloseVMCommand != null && CloseVMCommand.CanExecute(null);
        }


        void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (CloseVMCommand != null && CloseVMCommand.CanExecute(null))
            {
                CloseVMCommand.Execute(null);
            }
        }


        void Close_CanExecuteChanged(object sender, EventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }


        void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AssociatedObject.Close();
        }
    }
}
