using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Management;
using System.Timers;

namespace quickIpSettings
{
    public partial class MainWindow : Window
    {
        public const string GREY_BG = "#BFBDC1";
        public const string WHITE_BG = "#FFFFFF";
        public const string APPLYBUTTON_BG = "#ACAD94";
        public const string APPLYBUTTON_ALT_BG = "#697A21";

        private System.Timers.Timer ApplyButtonTimer;

        public MainWindow()
        {
            InitializeComponent();
            // Time for changing the apply button color after successfully applying new settings
            ApplyButtonTimer = new System.Timers.Timer(1500);
        }

        private void ResetApplyButtonColor(object state)
        {
            throw new NotImplementedException();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Reset the current adapter information before each scan & update
            while ( AdapterStackPanel.Children.Count > 0 )
            {
                AdapterStackPanel.Children.RemoveAt(0);
            }

            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();

            short adapterIndex = 0;

            foreach (NetworkInterface adapter in adapters)
            {
                // Only show adapters supporting IPv4
                // I is assumed that loopback adapters are not of interest

                
                if (adapter.NetworkInterfaceType == NetworkInterfaceType.Loopback || (! adapter.Supports(NetworkInterfaceComponent.IPv4)) )
                {
                    continue;
                }
                
                IPInterfaceProperties properties = adapter.GetIPProperties();
                IPv4InterfaceProperties IPv4properties = properties.GetIPv4Properties();

                LinkedList<string> adapterIPAddresses = new LinkedList<string>();
                LinkedList<string> adapterSubnetMasks = new LinkedList<string>();
                LinkedList<string> adapterGateways = new LinkedList<string>();
                adapterIPAddresses.Clear();
                adapterSubnetMasks.Clear();
                adapterGateways.Clear();

                foreach (UnicastIPAddressInformation ip in properties.UnicastAddresses)
                {
                    // Check that the interface is in use and that the ip address type is IPv4,
                    // others are not interesting in this scope
                    if ( adapter.OperationalStatus == OperationalStatus.Up &&
                         ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork )
                    {
                        adapterIPAddresses.AddLast(ip.Address.ToString());
                        adapterSubnetMasks.AddLast(ip.IPv4Mask.ToString());
                    }
                }

                foreach (GatewayIPAddressInformation gateway in properties.GatewayAddresses)
                {
                    adapterGateways.AddLast(gateway.Address.ToString());
                }

                Border AdapterCanvasBorder = new Border();
                AdapterCanvasBorder.BorderThickness = new Thickness(1);


                Canvas AdapterCanvas = new Canvas();
                AdapterCanvas.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#D8D4D5");
                AdapterCanvas.Height = 40;
                AdapterCanvas.Width = 753;
                AdapterCanvas.HorizontalAlignment = HorizontalAlignment.Left;
                AdapterCanvas.Name = "adapterData_" + adapterIndex;

                //Label adapterName = new Label();
                TextBlock adapterName = new TextBlock();
                Label IPLabel = new Label();
                Label netmaskLabel = new Label();
                Label gatewayLabel = new Label();
                Label DHCPLabel = new Label();

                TextBox AdapterIPTextBox = new TextBox();
                TextBox NetmaskTextBox = new TextBox();
                TextBox GatewayTextBox = new TextBox();
                CheckBox DHCPCheckBox = new CheckBox();

                Button applyButton = new Button();

                // Dummy initialization values
                string adapterIPAddress = "0.0.0.0";
                string adapterNetmask = "1.1.1.1";
                string adapterGateway = "3.3.3.3";

                // In the following it is assumed that only one IPv4 address, subnet mask and
                // gateway, respectively, is of interest
                if ( adapterIPAddresses.Count < 1 )
                {
                    adapterIPAddress = "null";
                }
                else
                {
                    adapterIPAddress = adapterIPAddresses.ElementAt(0);
                }

                if (adapterSubnetMasks.Count < 1)
                {
                    adapterNetmask = "null";
                }
                else
                {
                    adapterNetmask = adapterSubnetMasks.ElementAt(0);
                }

                if (adapterGateways.Count < 1)
                {
                    adapterGateway = "null";
                }
                else
                {
                    adapterGateway = adapterGateways.ElementAt(0);
                }

                AdapterIPTextBox.Text = adapterIPAddress;
                AdapterIPTextBox.Name = "AdapterIP_" + adapterIndex;
                AdapterIPTextBox.Margin = new Thickness(210, 5, 300, 5);
                AdapterIPTextBox.MinWidth = 80;
                AdapterIPTextBox.MaxWidth = 80;

                NetmaskTextBox.Text = adapterNetmask;
                NetmaskTextBox.Name = "AdapterNetmask_" + adapterIndex;
                NetmaskTextBox.Margin = new Thickness(353, 5, 300, 5);
                NetmaskTextBox.MinWidth = 80;
                NetmaskTextBox.MaxWidth = 80;

                GatewayTextBox.Text = adapterGateway;
                GatewayTextBox.Name = "AdapterGateway_" + adapterIndex;
                GatewayTextBox.Margin = new Thickness(503, 5, 300, 5);
                GatewayTextBox.MinWidth = 80;
                GatewayTextBox.MaxWidth = 80;

                DHCPCheckBox.Margin = new Thickness(650, 7, 0, 5);
                DHCPCheckBox.IsChecked = IPv4properties.IsDhcpEnabled;
                DHCPCheckBox.Name = "AdapterDHCPon_" + adapterIndex;

                if ( DHCPCheckBox.IsChecked == true )
                {
                    AdapterIPTextBox.IsReadOnly = true;
                    NetmaskTextBox.IsReadOnly = true;
                    GatewayTextBox.IsReadOnly = true;

                    AdapterIPTextBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(GREY_BG);
                    NetmaskTextBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(GREY_BG);
                    GatewayTextBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(GREY_BG);
                }
                else
                {
                    AdapterIPTextBox.IsReadOnly = false;
                    NetmaskTextBox.IsReadOnly = false;
                    GatewayTextBox.IsReadOnly = false;

                    AdapterIPTextBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(WHITE_BG);
                    NetmaskTextBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(WHITE_BG);
                    GatewayTextBox.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(WHITE_BG);
                }

                DHCPCheckBox.Click += DHCPCheckBox_Click;


                adapterName.Name = "adapter_" + adapterIndex;
                adapterName.Text = adapter.Description;
                adapterName.FontSize = 10;
                adapterName.Width = 190;
                adapterName.TextWrapping = TextWrapping.WrapWithOverflow;

                IPLabel.Content = "IP:";
                IPLabel.Margin = new Thickness(187, 0, 0, 0);

                netmaskLabel.Content = "Netmask:";
                netmaskLabel.Margin = new Thickness(295, 0, 0, 0);

                gatewayLabel.Content = "Gateway:";
                gatewayLabel.Margin = new Thickness(445, 0, 0, 0);

                DHCPLabel.Content = "DHCP on:";
                DHCPLabel.Margin = new Thickness(590, 0, 0, 0);

                applyButton.Content = "Apply changes";
                applyButton.FontSize = 10;
                applyButton.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(APPLYBUTTON_BG);
                applyButton.Margin = new Thickness(680, 5, 0, 0);
                applyButton.Width = 70;
                applyButton.Name = "applyButton_" + adapterIndex;
                applyButton.Click += applyButton_Click;

                // Bind UI element hierarchy
                AdapterCanvas.Children.Add(adapterName);
                AdapterCanvas.Children.Add(IPLabel);
                AdapterCanvas.Children.Add(netmaskLabel);
                AdapterCanvas.Children.Add(NetmaskTextBox);
                AdapterCanvas.Children.Add(gatewayLabel);
                AdapterCanvas.Children.Add(GatewayTextBox);
                AdapterCanvas.Children.Add(DHCPLabel);
                AdapterCanvas.Children.Add(DHCPCheckBox);
                AdapterCanvas.Children.Add(AdapterIPTextBox);
                AdapterCanvas.Children.Add(applyButton);
                AdapterCanvasBorder.Child = AdapterCanvas;
                AdapterStackPanel.Children.Add(AdapterCanvasBorder);
                
                // Update adapter index for next iteration
                ++adapterIndex;
            }
        }

        private void DHCPCheckBox_Click(object sender, RoutedEventArgs e)
        {
            
            CheckBox cb = sender as CheckBox;
            
            // Find correct index for the adapter
            int index = Int32.Parse(cb.Name.Split('_').ElementAt(1));

            // The correct element is found by searching the hierarchy down iteratively
            TextBox AdapterIP = VisualTreeHelpers.FindChild<TextBox>(AdapterStackPanel, ("AdapterIP_" + index));
            TextBox AdapterNetmask = VisualTreeHelpers.FindChild<TextBox>(AdapterStackPanel, ("AdapterNetmask_" + index));
            TextBox AdapterGateway = VisualTreeHelpers.FindChild<TextBox>(AdapterStackPanel, ("AdapterGateway_" + index));

            if (cb.IsChecked == true)
            {
                AdapterIP.IsReadOnly = true;
                AdapterNetmask.IsReadOnly = true;
                AdapterGateway.IsReadOnly = true;

                AdapterIP.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(GREY_BG);
                AdapterNetmask.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(GREY_BG);
                AdapterGateway.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(GREY_BG);
            }
            else
            {
                AdapterIP.IsReadOnly = false;
                AdapterNetmask.IsReadOnly = false;
                AdapterGateway.IsReadOnly = false;

                AdapterIP.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(WHITE_BG);
                AdapterNetmask.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(WHITE_BG);
                AdapterGateway.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(WHITE_BG);
            }

        }

        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            // Find correct index for the adapter
            int index = Int32.Parse(btn.Name.Split('_').ElementAt(1));

            // The correct element is found by searching the hierarchy down iteratively
            TextBlock adapterDescription = VisualTreeHelpers.FindChild<TextBlock>(AdapterStackPanel, ("adapter_" + index));
            TextBox AdapterIP = VisualTreeHelpers.FindChild<TextBox>(AdapterStackPanel, ("AdapterIP_" + index));
            TextBox AdapterNetmask = VisualTreeHelpers.FindChild<TextBox>(AdapterStackPanel, ("AdapterNetmask_" + index));
            TextBox AdapterGateway = VisualTreeHelpers.FindChild<TextBox>(AdapterStackPanel, ("AdapterGateway_" + index));
            CheckBox DHCPselection = VisualTreeHelpers.FindChild<CheckBox>(AdapterStackPanel, ("AdapterDHCPon_" + index));

            ManagementClass adapterMC = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection adapterCollection = adapterMC.GetInstances();

            foreach ( ManagementObject adapter in adapterCollection )
            {
                if ( String.Equals(adapter["Description"], adapterDescription.Text) )
                {
                    // Enable static IP settings or DHCP based on the checkbox selection
                    if ( DHCPselection.IsChecked == false )
                    {
                        try
                        {
                            // Set Default Gateway
                            var newGateway = adapter.GetMethodParameters("SetGateways");

                            // First set up the gateways with an empty array to clear the existing gateways
                            // It is assumed here that one gateway per adapter is sufficient
                            newGateway["DefaultIPGateway"] = new string[] { };
                            newGateway["GatewayCostMetric"] = new int[] { 1 };
                            adapter.InvokeMethod("SetGateways", newGateway, null); 

                            // Then add the actual gateway
                            newGateway["DefaultIPGateway"] = new string[] { AdapterGateway.Text };
                            adapter.InvokeMethod("SetGateways", newGateway, null);

                            // Set IP address and Subnet Mask
                            var newAddress = adapter.GetMethodParameters("EnableStatic");
                            newAddress["IPAddress"] = new string[] { AdapterIP.Text };
                            newAddress["SubnetMask"] = new string[] { AdapterNetmask.Text };
                            adapter.InvokeMethod("EnableStatic", newAddress, null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Unable to set static IP:\n" + ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            // Clear the gateway list before DHCP updates it so that
                            // no unintended gateways remain in the list
                            var newGateway = adapter.GetMethodParameters("SetGateways");
                            newGateway["DefaultIPGateway"] = new string[] { };
                            newGateway["GatewayCostMetric"] = new int[] { 1 };
                            adapter.InvokeMethod("SetGateways", newGateway, null);

                            adapter.InvokeMethod("EnableDHCP", null);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Unable to set dynamic IP:\n" + ex.Message);
                        }

                    }

                    // Adapt the callback function to the ElapsedEventHandler format with a lambda expression
                    ApplyButtonTimer.Elapsed += (sender2, e2) => changeApplyButtonColor(sender2, e2, btn);

                    btn.Content = "Applied";
                    btn.FontSize = 10;
                    btn.Margin = new Thickness(680, 5, 0, 0);
                    btn.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(APPLYBUTTON_ALT_BG);

                    ApplyButtonTimer.Stop();
                    ApplyButtonTimer.Start();


                    break; // Further iteration unnecessary
                }

            }

        }

        private void changeApplyButtonColor(object sender, ElapsedEventArgs e, Button btn)
        {
            System.Timers.Timer timer = sender as System.Timers.Timer;
            timer.Stop();

            // Dispatch the change back to original format
            Dispatcher.Invoke(() =>
            {
                btn.Content = "Apply changes";
                btn.FontSize = 10;
                btn.Margin = new Thickness(680, 5, 0, 0);
                btn.Background = (SolidColorBrush)new BrushConverter().ConvertFromString(APPLYBUTTON_BG);
            });
        }
    }

    public class VisualTreeHelpers
    {

        // Original highly useful code was found here: https://rachel53461.wordpress.com/2011/10/09/navigating-wpfs-visual-tree/
        // All credit to Rachel Lim

        public static T FindAncestor<T>(DependencyObject current)
        where T : DependencyObject
        {
            current = VisualTreeHelper.GetParent(current);

            while (current != null)
            {
                if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };
            return null;
        }

        /// <summary>
        /// Returns a specific ancester of an object
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, T lookupItem)
        where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T && current == lookupItem)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };
            return null;
        }

        /// <summary>
        /// Finds an ancestor object by name and type
        /// </summary>
        public static T FindAncestor<T>(DependencyObject current, string parentName)
        where T : DependencyObject
        {
            while (current != null)
            {
                if (!string.IsNullOrEmpty(parentName))
                {
                    var frameworkElement = current as FrameworkElement;
                    if (current is T && frameworkElement != null && frameworkElement.Name == parentName)
                    {
                        return (T)current;
                    }
                }
                else if (current is T)
                {
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            };

            return null;

        }

        /// <summary>
        /// Looks for a child control within a parent by name
        /// </summary>
        public static T FindChild<T>(DependencyObject parent, string childName)
        where T : DependencyObject
        {
            // Confirm parent and childName are valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                    else
                    {
                        // recursively drill down the tree
                        foundChild = FindChild<T>(child, childName);

                        // If the child is found, break so we do not overwrite the found child.
                        if (foundChild != null) break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Looks for a child control within a parent by type
        /// </summary>
        public static T FindChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            // Confirm parent is valid.
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child);

                    // If the child is found, break so we do not overwrite the found child.
                    if (foundChild != null) break;
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }
            return foundChild;
        }
    }
}
