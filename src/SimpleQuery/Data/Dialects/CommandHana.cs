using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SimpleQuery.Data.Dialects
{
    /// <summary>
    /// ExecuteNonQuery, ExecuteScalar and ExecuteReader, specifically for the Hana.
    /// </summary>
    public class CommandHana
    {
        [DllImport("user32", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_LBUTTONUP = 0x202;

        public int ExecuteNonQuery(IDbCommand command)
        {
            var result = 0;
            var task = Task.Factory.StartNew(() => result = command.ExecuteNonQuery());

            WaitingTask(task);

            return result;
        }

        public object ExecuteScalar(IDbCommand command)
        {
            object result = null;
            var task = Task.Factory.StartNew(() => result = command.ExecuteScalar());

            WaitingTask(task);

            return result;
        }

        public IDataReader ExecuteReader(IDbCommand command)
        {
            IDataReader data = null;

            var task = Task.Factory.StartNew(() => data = command.ExecuteReader());

            WaitingTask(task);

            return data;
        }

        private void WaitingTask(Task task)
        {
            while (task.Status <= TaskStatus.Running)
            {
                if (task.Status == TaskStatus.Running)
                {
                    CloseAssertMessageBox();
                }
            }

            if (task.IsFaulted)
            {
                if (task.Exception != null)
                {
                    throw task.Exception;
                }
            }
        }

        private void CloseAssertMessageBox()
        {
            var window = FindWindow(null, "Abort execution?");
            var button = FindWindowEx(window, IntPtr.Zero, "Button", "&No");
            SendMessage(button, WM_LBUTTONDOWN, 0, 0);
            SendMessage(button, WM_LBUTTONUP, 0, 0);
            SendMessage(button, WM_LBUTTONDOWN, 0, 0);
            SendMessage(button, WM_LBUTTONUP, 0, 0);
        }
    }
}