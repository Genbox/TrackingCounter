﻿using System;
using System.Diagnostics;
using System.Windows.Forms;
using SharpPcap;
using SharpPcap.WinPcap;

namespace TrackingCounter
{
    public partial class MainForm : Form
    {
        private long _packetCount;
        private WinPcapDevice _selectedDevice;
        private DateTime _startDate;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Retrieve the device list
            CaptureDeviceList devices = CaptureDeviceList.Instance;

            // If no devices were found print an error
            if (devices.Count >= 1)
            {
                int bestCandidate = 0;
                int counter = 0;

                foreach (WinPcapDevice captureDevice in devices)
                {
                    if (captureDevice.Interface.Addresses.Count >= 1 && captureDevice.Loopback == false)
                    {
                        cbDevices.Items.Add(new DeviceWrapper(captureDevice));

                        string gateway = captureDevice.Interface.GatewayAddress == null ? string.Empty : captureDevice.Interface.GatewayAddress.ToString();
                        if (!string.IsNullOrEmpty(gateway) && gateway != "0.0.0.0")
                            bestCandidate = counter;

                        counter++;
                    }
                }

                if (cbDevices.Items.Count >= 1)
                {
                    cbDevices.SelectedIndex = bestCandidate;
                }
            }

            _startDate = DateTime.Now;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.OnPacketArrival -= SelectedDevice_OnPacketArrival;
                _selectedDevice.StopCapture();
                _selectedDevice.Close();
            }
        }

        private void cbDevices_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_selectedDevice != null)
            {
                _selectedDevice.OnPacketArrival -= SelectedDevice_OnPacketArrival;
                _selectedDevice.StopCapture();
                _selectedDevice.Close();
            }

            _selectedDevice = ((DeviceWrapper)cbDevices.SelectedItem).Device;
            _selectedDevice.OnPacketArrival += SelectedDevice_OnPacketArrival;

            _selectedDevice.Open(DeviceMode.Normal);
            _selectedDevice.Filter = "net not 10.0.0.0/8 and net not 172.16.0.0/12 and net not 192.168.0.0/24";
            _selectedDevice.StartCapture();

            _packetCount = 0;
        }

        private void SelectedDevice_OnPacketArrival(object sender, CaptureEventArgs e)
        {
            if (InvokeRequired)
                Invoke(new Action(UpdatePacketCount));
            else
                UpdatePacketCount();
        }

        private void UpdatePacketCount()
        {
            if (DateTime.Now.Date.DayOfYear != _startDate.DayOfYear)
            {
                _startDate = DateTime.Now;
                _packetCount = 0;
            }

            _packetCount++;
            long registrations = (_packetCount / 500);
            lblTrackingCount.Text = registrations.ToString();

            notifyIcon1.Text = registrations.ToString() + " registrationer i dag";
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://logning.bitbureauet.dk/");
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (Visible)
                Hide();
            else
                Show();
        }
    }
}
