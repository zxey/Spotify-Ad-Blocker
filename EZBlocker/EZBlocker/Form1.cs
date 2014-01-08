﻿using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace EZBlocker
{
    public partial class Main : Form
    {
        private String title = String.Empty; // Title of the Spotify window
        private Boolean autoAdd = true;

        private String blocklistPath = Application.StartupPath + @"\blocklist.txt";
        private String nircmdPath = Application.StartupPath +@"\nircmdc.exe";

        private String website = @"http://www.ericzhang.me/projects/spotify-ad-blocker-ezblocker/";

        public Main()
        {
            InitializeComponent();
            if (!File.Exists(nircmdPath))
                File.WriteAllBytes(nircmdPath, EZBlocker.Properties.Resources.nircmdc);
            if (!File.Exists(blocklistPath))
                File.WriteAllText(blocklistPath, GetPage("http://www.ericzhang.me/dl/?file=blocklist.txt"));
            try
            {
                Process.Start(Environment.GetEnvironmentVariable("APPDATA") + @"\Spotify\spotify.exe");
            }
            catch (Exception e)
            {
                // Ignore
            }
        }

        private void MainTimer_Tick(object sender, EventArgs e)
        {
            UpdateTitle();
        }

        /**
         * Updates the title of the Spotify window.
         * 
         * Returns true if title updated successfully, false if otherwise
         **/
        private Boolean UpdateTitle()
        {
            Process[] p = Process.GetProcesses();
            for (var i = 0; i < p.Length; i++)
            {
                if (p[i].ProcessName.Equals("spotify"))
                {
                    title = p[i].MainWindowTitle;
                    return true;
                }
            }
            return false;
        }

        /**
         * Determines whether or not Spotify is currently playing
         **/
        private Boolean IsPlaying()
        {
            return title.Contains(" - ");
        }

        /**
         * Returns the current artist
         **/
        private String GetArtist()
        {
            if (!IsPlaying()) return String.Empty;
            return title.Substring(10).Split('\u2013')[0].TrimEnd(); // Split at endash
        }

        /**
         * Adds an artist to the blocklist.
         * 
         * Returns false if Spotify is not playing.
         **/
        private Boolean AddToBlockList(String artist)
        {
            if (!IsPlaying()) return false;
            File.AppendAllText(blocklistPath, artist + "\r\n");
            return true;
        }

        /**
         * Attempts to check if the current song is an ad
         * 
         * Checks with iTunes API, can also use http://ws.spotify.com/search/1/artist?q=artist
         **/
        private Boolean IsAd(String artist)
        {
            return GetPage("http://itunes.apple.com/search?entity=musicArtist&limit=1&term=" + artist.Replace(" ", "+")).Contains("\"resultCount\":0");
        }

        /**
         * Checks if an artist is in the blocklist (Exact match only)
         **/
        private Boolean IsBlocked(String artist)
        {
            String[] lines = File.ReadAllLines(blocklistPath);
            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].Equals(artist))
                    return true;
            }
            return false;
        }

        /**
         * Gets the source of a given URL
         **/
        private String GetPage(String URL)
        {
            WebClient w = new WebClient();
            w.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.2; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/32.0.1667.0 Safari/537.36");
            String s = w.DownloadString(URL);
            
            return s;
        }

        private void BlockButton_Click(object sender, EventArgs e)
        {
            AddToBlockList(GetArtist());
        }

        private void AutoAddCheck_CheckedChanged(object sender, EventArgs e)
        {
            autoAdd = AutoAddCheck.Checked;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            Process.Start("notepad.exe", blocklistPath);
        }

        private void MuteButton_Click(object sender, EventArgs e)
        {

        }

    }
}