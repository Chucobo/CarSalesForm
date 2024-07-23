/*
Bao Tran

7/11/2023

assignment CarSalesV2 
    
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CarSalesV2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CarSalesV2 : Page
    {
        const double GST_RATE = 0.1;
        const double WARRANTY_1YEAR = 0;
        const double WARRANTY_2YEAR = .05;
        const double WARRANTY_3YEAR = .1;
        const double WARRANTY_5YEAR = .2;
        const double WINDOW_TINT = 150, DUCO_PROTECTION = 180, FLOOR_MATS = 320, DELUXE_SOUND_SYSTEM = 350;
        const double INSURANCE_UNDER25 = .20, INSURANCE_OVER25 = .10;


        public CarSalesV2()
        {
            this.InitializeComponent();
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            //Reset input fields
            customerNumberTextBox.Text = "";
            customerNameTextBox.Text = "";
            vehiclePriceTextBox.Text = "";
            tradeInTextBox.Text = "";
            subAmountTextBox.Text = "";
            gstAmountTextBox.Text = "";
            finalAmountTextBox.Text = "";
            summaryTextBlock.Text = "";
            customerNumberTextBox.IsEnabled = true;
            customerNameTextBox.IsEnabled = true;
            customerNameTextBox.Focus(FocusState.Programmatic);
            warrantyComboBox.SelectedIndex = 0;
            windowTintingCheckBox.IsChecked = false;
            ducoProtectionCheckBox.IsChecked = false;   
            floorMatsCheckBox.IsChecked = false;    
            deluxeSoundSystemCheckBox.IsChecked = false;
            insuranceToggleSwitch.IsOn = false;


        }

        private async void summaryButton_Click(object sender, RoutedEventArgs e)
        {
            double vehiclePrice = 0;
            double tradeInAmount = 0;
            double subAmount = 0;
            double gstAmount;
            double finalAmount;
            double insuranceCost, warrantyCost, optionalExtras;
            //Check valid number for Vehicle Price
            try
            {
                vehiclePrice = double.Parse(vehiclePriceTextBox.Text);
            }
            catch (Exception)
            {
                var dialogMessage = new MessageDialog("Error! Please enter a valid whole number for Vehicle Price");
                await dialogMessage.ShowAsync();
                vehiclePriceTextBox.Focus(FocusState.Programmatic);
                return;
            }
            //Check if trade in field is empty then set to 0
            if (tradeInTextBox.Text == "")
            {
                tradeInTextBox.Text = "0";
            }

            //Check valid number for Trade In 
            try
            {
                tradeInAmount = double.Parse(tradeInTextBox.Text);
            }
            catch (Exception)
            {
                var dialogMessage = new MessageDialog("Error! Please enter a valid whole number for Trade In Price");
                await dialogMessage.ShowAsync();
                tradeInTextBox.Focus(FocusState.Programmatic);
                return;
            }
            //validate Vehicle amount is > 0
            if (vehiclePrice <= 0)
            {
                var dialogMessage = new MessageDialog("Error! Vehicle Amount has to be greater 0");
                await dialogMessage.ShowAsync();
                return;
            }
            //validate Trade in amount is => 0
            if (tradeInAmount < 0)
            {
                var dialogMessage = new MessageDialog("Error! Trade in price has to be less than 0");
                await dialogMessage.ShowAsync();
                return;
            }
            //Validate Vehicle amount is > Trade in amount
            if (vehiclePrice < tradeInAmount)
            {
                var dialogMessage = new MessageDialog("Error! Trade in amount cannot be greater than Vehicle Amount");
                await dialogMessage.ShowAsync();
                return;
            }

            warrantyCost = calcVehicleWarranty(vehiclePrice);
            optionalExtras = calcOptionsExtras();
            insuranceCost = calcAccidentInsurance(vehiclePrice, optionalExtras);
            subAmount = vehiclePrice + warrantyCost + optionalExtras + insuranceCost - tradeInAmount;

            gstAmount = subAmount * GST_RATE;
            subAmountTextBox.Text = subAmount.ToString("C");
            gstAmountTextBox.Text = gstAmount.ToString("C");
            finalAmount = subAmount + gstAmount;
            finalAmountTextBox.Text = finalAmount.ToString("C");
            summaryTextBlock.Text = "Customer Name:  " + customerNameTextBox.Text + "\nCustomer Number:  " + customerNumberTextBox.Text +
                "\nVehicle Price:  " + vehiclePrice.ToString("C") +
                "\nTrade-In Amount:  " + tradeInAmount.ToString("C") +
                "\nWarranty Cost:  " + warrantyCost.ToString("C") +
                "\nOptional Extras Cost:  " + optionalExtras.ToString("C") +
                "\nInsurance Cost:  " + insuranceCost.ToString("C") +
                "\n\n\nFinal Amount:  " + finalAmount.ToString("C");
        }

        private void insuranceToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if(insuranceToggleSwitch.IsOn)
            {
                insuranceOver25RadioButton.IsEnabled = true;
                insuranceUnder25RadioButton.IsEnabled = true;
                insuranceUnder25RadioButton.IsChecked = true;
            }   
            else
            {
                insuranceOver25RadioButton.IsEnabled = false;
                insuranceUnder25RadioButton.IsEnabled = false;
                insuranceUnder25RadioButton.IsChecked = false;
                insuranceOver25RadioButton.IsChecked = false;
            }
        }

        private async void saveButton_Click(object sender, RoutedEventArgs e)
        {
            //Check if Name is empty
            if (customerNameTextBox.Text == "")
            {
                var dialogMessage = new MessageDialog("Error! Customer Name cannot be blank");
                await dialogMessage.ShowAsync();
                customerNameTextBox.Focus(FocusState.Programmatic);
                return;
            }

            //Check if Phone number is empty
            if (customerNumberTextBox.Text == "")
            {
                var dialogMessage = new MessageDialog("Error! Customer Number cannot be blank");
                await dialogMessage.ShowAsync();
                customerNumberTextBox.Focus(FocusState.Programmatic);
                return;

            }

            customerNumberTextBox.IsEnabled = false;
            customerNameTextBox.IsEnabled = false;
            vehiclePriceTextBox.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Calculates Vehicle Warranty price and returns the price
        /// </summary>
        /// <param name="vehiclePrice"></param>
        /// <returns> Returns cost of warranty </returns>
        private double calcVehicleWarranty(double vehiclePrice)
        {
            double warranty = 0;
            int selectedIndex = warrantyComboBox.SelectedIndex;
            switch (selectedIndex)
            {
                case 0:
                    warranty = vehiclePrice * WARRANTY_1YEAR;
                    break;
                case 1:
                    warranty = vehiclePrice * WARRANTY_2YEAR;
                    break;
                case 2:
                    warranty = vehiclePrice * WARRANTY_3YEAR;
                    break;
                case 3:
                    warranty =vehiclePrice * WARRANTY_5YEAR;
                    break;

            }
            return warranty;
        }

        /// <summary>
        /// Calculates total costs of optional extras and returns the price
        /// </summary>
        /// <returns> Returns total cost of optional extras selected </returns>
        private double calcOptionsExtras()
        {
            double extras=0;
            if (windowTintingCheckBox.IsChecked == true) 
            {
                extras = WINDOW_TINT;
            }
            if (ducoProtectionCheckBox.IsChecked == true)
            {
                extras += DUCO_PROTECTION;
            }
            if (floorMatsCheckBox.IsChecked == true)
            {
                extras += FLOOR_MATS;
            }
            if(deluxeSoundSystemCheckBox.IsChecked == true)
            {
                extras += DELUXE_SOUND_SYSTEM;
            }
            return extras;
        }
        /// <summary>
        /// Calculates insurance cost 
        /// </summary>
        /// <param name="vehiclePrice"></param>
        /// <param name="optionalExtras"></param>
        /// <returns> Returns total insurance cost </returns>
        private double calcAccidentInsurance(double vehiclePrice, double optionalExtras)
        {
            double insurance = 0;
            if (insuranceToggleSwitch.IsOn == true)
            {

                if (insuranceUnder25RadioButton.IsChecked == true)
                {
                    insurance = (vehiclePrice + optionalExtras) * INSURANCE_UNDER25;
                }
                else
                {
                    insurance = (vehiclePrice + optionalExtras) * INSURANCE_OVER25;
                }

            }
            return insurance;
        }
    }
}
