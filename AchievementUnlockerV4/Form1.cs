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
                console.LoadModule("HDD:\\cheesedick.xex");

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

                console.CallVoid(0x91E02258, numAchievements, achievementPtr, pOverlapped, pUserBuffer);
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
    }
}
