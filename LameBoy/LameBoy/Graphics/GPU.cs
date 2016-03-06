﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SDL2;

namespace LameBoy.Graphics
{
    public class GPU
    {
        SDLThread sdlt;
        Cart cart;
        byte[,] frame;
        List<byte[,]> tiles;

        public GPU(IntPtr Handle, IntPtr pgHandle)
        {
            sdlt = new SDLThread(Handle, pgHandle, this);
            Thread sdlThread = new Thread(new ThreadStart(sdlt.Render));
            sdlThread.Start();
            frame = new byte[160,144];
            tiles = new List<byte[,]>();
        }

        public void SetCPUExecutionState(bool state)
        {
            sdlt.rt.CPUexecuting = state;
        }

        public void SetCart(Cart NewCart)
        {
            cart = NewCart;
        }

        public void UpdateYCounter(byte count)
        {
            if(cart != null)
            {
                cart.Write8(0xFF44, count);
            }
        }
        
        private byte[,] DecodeTile(byte[] tile)
        {
            BitArray tileBits = new BitArray(tile);
            byte[,] lines = new byte[8, 8];
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    int highBitPos = ((7 - x) + 8 + (y * 16));
                    int lowBitPos = (7 - x) + (y * 16);
                    byte high = (byte)(Convert.ToByte(tileBits.Get(highBitPos)) << 1);
                    byte low = Convert.ToByte(tileBits.Get(lowBitPos));
                    byte color = (byte)(high | low);
                    lines[y, x] = color;
                }
            }
            return lines;
        }

        private void PushFrame()
        {
            sdlt.rt.SetPixels(frame);
        }

        private void DrawTile(byte[,] tile, int xCoord, int yCoord)
        {
            for (int y = 0; y < 144; y++)
            {
                for (int x = 0; x < 160; x++)
                {
                    if ((y > yCoord - 1 && y < yCoord + 8) && (x > xCoord - 1 && x < xCoord + 8))
                    {
                        frame[x, y] = tile[y - yCoord, x - xCoord];
                    }
                }
            }
        }

        public void RenderScene()
        {
            while (true)
            {
                if (cart == null)
                {
                    //This just fills the frame buffer with junk, to test it
                    for (int y = 0; y < 144; y++)
                    {
                        for (int x = 0; x < 160; x++)
                        {
                            frame[x, y] = (byte)((x + y) % 4);
                        }
                    }
                    PushFrame();
                }
                else
                {
                    //Render the scene here
                    //Once CPU is implemented, this will copy each tile object from
                    //VRAM into a byte array, and each will be rendered according to
                    //data stored in OAM
                    for (int n = 0; n < 0xFF; n++)
                    {
                        byte[] tile = new byte[16];
                        for (int i = 0; i < 16; i++)
                        {
                            //4329 = 1
                            //378F = square block
                            tile[i] = cart.Read8(0x8000 + (n * 0x10) + i);
                        }
                        tiles.Add(DecodeTile(tile));
                    }

                    for(int y = 0; y < 0x20; y++)
                    {
                        for(int x = 0; x < 0x20; x++)
                        {
                            byte[,] tile = tiles.ElementAt(cart.Read8(0x9800 + x + (y * 0x20)));
                            if(x < 0x14 && y < 0x14)
                                DrawTile(tile, x * 8, y * 8);
                        }
                    }

                    //for(int n = 0; n < 0xFF; n++)
                   // {
                        //byte[,] tile = tiles.ElementAt(n);
                        //DrawTile(tile, (n * 8) % 160, ((int) n / 20) * 8);
                    //}

                    PushFrame();
                }
            }
        }
    }
}
