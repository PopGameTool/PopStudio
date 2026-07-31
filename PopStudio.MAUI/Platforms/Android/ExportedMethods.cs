using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using System;
using System.IO;
using System.Threading.Tasks;
using PopStudio.Package.Rsb;
using PopStudio.Platform;

namespace PopStudio.MAUI
{
    [Service(Exported = true, Name = "com.yingfengtingyu.popstudio.ExportedService")]
    [IntentFilter(new[] { "com.yingfengtingyu.popstudio.ACCESS_SERVICE" })]
    public class ExportedService : Service
    {
        // 约定的消息指令码
        private const int MsgRsbPack = 1;
        private const int MsgRsbUnpack = 2;

        private class IncomingHandler : Handler
        {
            private readonly Context _context;

            public IncomingHandler(Context context)
            {
                _context = context;
            }

            public override void HandleMessage(Message msg)
            {
                // 1. 复制一份 Message，防止被系统回收
                Message msgCopy = Message.Obtain(msg);

                // 2. 开启后台线程处理耗时任务 (避免阻塞主线程 Handler)
                Task.Run(async () =>
                {
                    int requestId = msgCopy.Data.GetInt("requestId", -1);
                    Bundle resultBundle = new Bundle();
                    // 回传 RequestId，这是匹配的关键
                    resultBundle.PutInt("requestId", requestId);

                    try
                    {
                        switch (msgCopy.What)
                        {
                            case MsgRsbPack:
                                await ProcessRsbPackAsync(msgCopy.Data, resultBundle);
                                break;
                            case MsgRsbUnpack:
                                await ProcessRsbUnpackAsync(msgCopy.Data, resultBundle);
                                break;
                            default:
                                resultBundle.PutString("result", "Unknown command");
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        resultBundle.PutString("result", $"Fatal error: {e.Message}");
                    }

                    // 3. 处理完毕，发送回复
                    SendReply(msgCopy, resultBundle);
                    msgCopy.Recycle();
                });
            }

            private async Task ProcessRsbPackAsync(Bundle inputData, Bundle resultBundle)
            {
                // 检查权限
                bool hasPermission = await CheckPermissionAsync(resultBundle);
                if (!hasPermission) return;

                try
                {
                    // 注意：从 inputData (客户端传来的) 读取参数，而不是 resultBundle
                    string inFolder = inputData.GetString("inFolder");
                    string outFile = inputData.GetString("outFile");

                    // 执行你的业务逻辑
                    Rsb.Pack(inFolder,
                        (outFile.EndsWith("_unpack") ? outFile[..^"_unpack".Length] : outFile) + ".rsb");
                    
                    resultBundle.PutString("result", "压缩成功");
                }
                catch (Exception e)
                {
                    resultBundle.PutString("result", "压缩失败: " + e.Message);
                }
            }

            private async Task ProcessRsbUnpackAsync(Bundle inputData, Bundle resultBundle)
            {
                bool hasPermission = await CheckPermissionAsync(resultBundle);
                if (!hasPermission) return;

                try
                {
                    string inFile = inputData.GetString("inFile");
                    string outFolder = inputData.GetString("outFolder");
                    bool changeImage = inputData.GetBoolean("changeimage", false);
                    bool delete = inputData.GetBoolean("delete", false);

                    Rsb.Unpack(inFile,
                        Path.Combine(Path.GetDirectoryName(outFolder), Path.GetFileNameWithoutExtension(outFolder)) + "_unpack",
                        changeImage,
                        delete);

                    resultBundle.PutString("result", "解压成功");
                }
                catch (Exception e)
                {
                    resultBundle.PutString("result", "解压失败: " + e.Message);
                }
            }

            private async Task<bool> CheckPermissionAsync(Bundle resultBundle)
            {
                bool hasPermission = await Permission.CheckPermissionAsync();
                resultBundle.PutBoolean("permission", hasPermission);
                return hasPermission;
            }

            private void SendReply(Message msg, Bundle data)
            {
                Messenger replyTo = msg.ReplyTo;
                if (replyTo != null)
                {
                    Message replyMsg = Message.Obtain();
                    replyMsg.Data = data;
                    try
                    {
                        replyTo.Send(replyMsg);
                    }
                    catch (RemoteException e)
                    {
                        Log.Error("PopStudioService", "Reply failed", e);
                    }
                }
            }
        }

        private Messenger _messenger;

        public override void OnCreate()
        {
            base.OnCreate();
            _messenger = new Messenger(new IncomingHandler(this));
        }

        public override IBinder OnBind(Intent intent)
        {
            return _messenger.Binder;
        }
    }
}