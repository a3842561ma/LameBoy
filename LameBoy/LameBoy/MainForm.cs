﻿using LameBoy.Graphics;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace LameBoy
{
    public partial class MainForm : Form
    {
        GameBoy gb;
        SDLThread rt;
        Debugger debuggerForm;

        readonly MenuItem[] scaleItems;

        public MainForm()
        {
            InitializeComponent();
            scaleItems = new[] { menuItem10, menuItem11, menuItem12, menuItem13, menuItem14 };
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            gb = new GameBoy();
            rt = new SDLThread(Handle, panelGraphics.Handle, gb);

            gb.HookRenderer(rt);

            debuggerForm = new Debugger(gb.CPU);

            scaleItems[gb.RenderThread.Scale - 1].Checked = true;
        }

        private void MainForm_Closing(object sender, FormClosingEventArgs e)
        {
            rt.Terminate();
            gb.Shutdown();
        }

        private void menuItemOpenRom_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();

            fd.Filter = "Game Boy ROMs (*.gb, *.gbc, *.bin, *.rom)|*.gb;*.gbc;*.bin;*.rom|All Files (*.*)|*.*";
            fd.FilterIndex = 1;

            fd.Multiselect = false;

            var okSelected = fd.ShowDialog();
            if (okSelected == DialogResult.OK)
            {
                Cart cart = new Cart(fd.FileName);
                gb.LoadCart(cart);

                Console.WriteLine(cart.GetCartType());

                if (File.ReadAllBytes(fd.FileName).Length == 0x10000 && cart.GetCartType() == CartType.ROM)
                {
                    //Doesn't execute CPU when loading a ramdump
                    return;
                }
                gb.Start();
                debuggerForm.Initialize();
            }
        }

        private void menuItem6_Click(object sender, EventArgs e)
        {
            //Set palette BGB
            Palette.SetColors(new byte[] { 0xE0, 0xF8, 0xD0 }, new byte[] { 0x88, 0xC0, 0x70 }, new byte[] { 0x34, 0x68, 0x56 }, new byte[] { 0x08, 0x18, 0x20 });
        }

        private void menuItem7_Click(object sender, EventArgs e)
        {
            //Set palette Dark Green
            Palette.SetColors(new byte[] { 156, 189, 15 }, new byte[] { 140, 173, 15 }, new byte[] { 48, 98, 48 }, new byte[] { 15, 56, 15 });
        }

        private void menuItem8_Click(object sender, EventArgs e)
        {
            //Set palette gray
            Palette.SetColors(new byte[] { 255, 255, 255 }, new byte[] { 192, 192, 192 }, new byte[] { 127, 127, 127 }, new byte[] { 0, 0, 0 });
        }

        private void menuItem15_Click(object sender, EventArgs e)
        {
            //Random Palette
            Random r = new Random();
            Palette.SetColors(new byte[] { (byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256) }, new byte[] { (byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256) }, new byte[] { (byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256) }, new byte[] { (byte)r.Next(256), (byte)r.Next(256), (byte)r.Next(256) });
        }

        private void menuItem10_Click(object sender, EventArgs e)
        {
            SetRenderScaleCheckedState(1);
        }

        private void menuItem11_Click(object sender, EventArgs e)
        {
            SetRenderScaleCheckedState(2);
        }

        private void menuItem12_Click(object sender, EventArgs e)
        {
            SetRenderScaleCheckedState(3);
        }

        private void menuItem13_Click(object sender, EventArgs e)
        {
            SetRenderScaleCheckedState(4);
        }

        private void menuItem14_Click(object sender, EventArgs e)
        {
            SetRenderScaleCheckedState(5);
        }

        private void SetRenderScaleCheckedState(int scale)
        {
            if (gb.RenderThread == null) return;

            scaleItems[gb.RenderThread.Scale - 1].Checked = false;
            scaleItems[scale - 1].Checked = true;

            gb.RenderThread.Scale = scale;
        }

        private void menuItem17_Click(object sender, EventArgs e)
        {
            if (debuggerForm.Visible)
                debuggerForm.Activate();
            else
                debuggerForm.Show(this);
        }
    }
}
