using JRPCPlusPlus;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using XDevkit;

namespace AchievementUnlockerV4
{
    public partial class Form1 : Form
    {
        IXboxConsole console;

        void UnlockAchievements()
        {
            try
            {
                byte[] file = Properties.Resources.cheesedick;
                string localName = Path.Combine(Path.GetTempPath(), "cheesedick.tmp");
                string remoteName = "HDD:\\cheesedick.xex";
                File.WriteAllBytes(localName, file);
                console.SendFile(localName, remoteName);
                console.LoadModule("HDD:\\cheesedick.xex");

                console.CallVoid(0x91E03000);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "cheesedick.tmp"));
                console.DeleteFile("HDD:\\cheesedick.xex");
            }
        }

        void UnlockAvatarAwards()
        {
            try
            {
                byte[] file = Properties.Resources.cheesedick;
                string localName = Path.Combine(Path.GetTempPath(), "cheesedick.tmp");
                string remoteName = "HDD:\\cheesedick.xex";
                File.WriteAllBytes(localName, file);
                console.SendFile(localName, remoteName);
                console.LoadModule("HDD:\\cheesedick.xex");

                console.CallVoid(0x91E03058);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                File.Delete(Path.Combine(Path.GetTempPath(), "cheesedick.tmp"));
                console.DeleteFile("HDD:\\cheesedick.xex");
            }
        }
        
        void XUserAwardGamerPicture()
        {
            uint address = console.ResolveFunction("xam.xex", 0x2F0); // XamWriteGamerTile
            uint dwUserIndex = 0;
            uint numPictures = 20;
            uint dwPictureId = 0;

            for (uint i = 0; i < numPictures; i++)
            {
                dwPictureId = i;
                console.CallVoid(address, dwUserIndex, 0, dwPictureId | 0x20000, dwPictureId | 0x10000, 1, 0);
                Thread.Sleep(30);
            }
        }

        public Form1()
        {
            InitializeComponent();
        }

        private void AchievementsBtn_Click(object sender, EventArgs e)
        {
            UnlockAchievements();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                if (!console.Connect(out console))
                    MessageBox.Show(Text = "Failed to connect to console!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void AvatarAwardsBtn_Click(object sender, EventArgs e)
        {
            UnlockAvatarAwards();
        }

        private void GamerPicsBtn_Click(object sender, EventArgs e)
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
