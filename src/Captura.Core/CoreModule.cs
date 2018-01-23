﻿using Captura.Models;
using Captura.ViewModels;
using Ninject.Modules;

namespace Captura
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            // Singleton View Models
            Bind<MainViewModel>().ToSelf().InSingletonScope();
            Bind<VideoViewModel>().ToSelf().InSingletonScope();
            Bind<AudioViewModel>().ToSelf().InSingletonScope();

            // Settings
            Bind<Settings>().ToSelf().InSingletonScope();

            // Localization
            Bind<LanguageManager>().ToMethod(M => LanguageManager.Instance).InSingletonScope();

            // Hotkeys
            Bind<HotKeyManager>().ToSelf().InSingletonScope();

            // Image Writers
            Bind<IImageWriterItem>().To<DiskWriter>().InSingletonScope();
            Bind<IImageWriterItem>().To<ClipboardWriter>().InSingletonScope();
            Bind<IImageWriterItem>().To<ImgurWriter>().InSingletonScope();

            // Video Writer Providers
            Bind<IVideoWriterProvider>().To<FFMpegWriterProvider>().InSingletonScope();
            Bind<IVideoWriterProvider>().To<GifWriterProvider>().InSingletonScope();
            Bind<IVideoWriterProvider>().To<StreamingWriterProvider>().InSingletonScope();

            // Check if SharpAvi is available
            if (ServiceProvider.FileExists("SharpAvi.dll"))
            {
                Bind<IVideoWriterProvider>().To<SharpAviWriterProvider>().InSingletonScope();
            }

            // Audio Source
            if (BassAudioSource.Available)
            {
                Bind<AudioSource>().To<BassAudioSource>().InSingletonScope();
            }
            else Bind<AudioSource>().To<NoAudioSource>().InSingletonScope();
        }
    }
}