﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;

namespace Microsoft.Maui
{
	public abstract class MauiWinUIApplication : UI.Xaml.Application
	{
		protected abstract MauiApp CreateMauiApp();

		protected override void OnLaunched(UI.Xaml.LaunchActivatedEventArgs args)
		{
			// Windows running on a different thread will "launch" the app again
			if (Application != null)
			{
				Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));
				Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
				return;
			}
			
			var mauiApp = CreateMauiApp();

			var rootContext = new MauiContext(mauiApp.Services);

			var applicationContext = rootContext.MakeApplicationScope(this);

			Services = applicationContext.Services;

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunching>(del => del(this, args));

			Application = Services.GetRequiredService<IApplication>();

			this.SetApplicationHandler(Application, applicationContext!);

			var winuiWndow = CreateNativeWindow(args, applicationContext!);

			winuiWndow.Activate();

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnLaunched>(del => del(this, args));
		}

		UI.Xaml.Window CreateNativeWindow(UI.Xaml.LaunchActivatedEventArgs args, IMauiContext applicationContext)
		{
			var winuiWndow = new MauiWinUIWindow();

			var mauiContext = applicationContext.MakeWindowScope(winuiWndow, out var windowScope);

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnMauiContextCreated>(del => del(mauiContext));

			var activationState = new ActivationState(mauiContext, args);
			var window = Application.CreateWindow(activationState);

			winuiWndow.SetWindowHandler(window, mauiContext);

			Services.InvokeLifecycleEvents<WindowsLifecycle.OnWindowCreated>(del => del(winuiWndow));

			return winuiWndow;
		}

		public static new MauiWinUIApplication Current => (MauiWinUIApplication)UI.Xaml.Application.Current;

		public IServiceProvider Services { get; protected set; } = null!;

		public IApplication Application { get; protected set; } = null!;
	}
}