using JRPCPlusPlus;
using System;
using System.IO;
using System.Windows.Forms;
using XDevkit;

namespace AchievementUnlockerV4
{
    public partial class Form1 : Form
    {
        IXboxConsole console;

        uint GetIdFromIndex(uint index)
        {
            return index + 1;
        }

        void XUserAwardGamerPicture()
        {
            uint address = console.ResolveFunction("xam.xex", 0x2F0);
            uint dwUserIndex = 0;
            uint numPictures = 20;
            uint dwPictureId = 0;

            for (uint i = 0; i < numPictures; i++)
            {
                dwPictureId = i;
                console.CallVoid(address, dwUserIndex, 0, dwPictureId | 0x20000, dwPictureId | 0x10000, 1, 0);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] file = Properties.Resources.cheesedick;
                string localName = Path.Combine(Path.GetTempPath(), "cheesedick.tmp");
                string remoteName = "HDD:\\Cache\\cheesedick.xex";
                File.WriteAllBytes(localName, file);
                console.SendFile(localName, remoteName);
                console.LoadModule("HDD:\\Cache\\cheesedick.xex");

                uint achievementPtr = 0x3A168860;
                uint pOverlapped = 0x3A168840;
                uint pUserBuffer = 0x3A168EA0;
                uint numAchievements = 200;
                ulong[] achievements = new ulong[numAchievements];

                for (uint i = 0; i < numAchievements; i++)
                {
                    achievements[i] = GetIdFromIndex(i);
                }

                console.WriteUInt64(achievementPtr, achievements);

                console.CallVoid(0x91E02340, numAchievements, achievementPtr, pOverlapped, pUserBuffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "cheesedick.tmp"));
                console.DeleteFile("HDD:\\Cache\\cheesedick.xex");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!console.Connect(out console))
                    MessageBox.Show(Text = "Failed to connect to console");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] file = Properties.Resources.cheesedick;
                string localName = Path.Combine(Path.GetTempPath(), "cheesedick.tmp");
                string remoteName = "HDD:\\Cache\\cheesedick.xex";
                File.WriteAllBytes(localName, file);
                console.SendFile(localName, remoteName);
                console.LoadModule("HDD:\\Cache\\cheesedick.xex");

                uint awardsPtr = 0x3A168860;
                uint pOverlapped = 0x3A168840;
                uint pUserBuffer = 0x3A168EA0;
                uint numAwards = 50;
                ulong[] awards = new ulong[numAwards];

                for (uint i = 0; i < numAwards; i++)
                {
                    awards[i] = GetIdFromIndex(i);
                }

                console.WriteUInt64(awardsPtr, awards);

                console.CallVoid(0x91E023B8, numAwards, awardsPtr, pOverlapped, pUserBuffer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "cheesedick.tmp"));
                console.DeleteFile("HDD:\\Cache\\cheesedick.xex");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                XUserAwardGamerPicture();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
