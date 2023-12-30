using Microsoft.Extensions.DependencyInjection;
using MTCG.Controller;
using MTCG.Data;
using MTCG.HttpServer;
using MTCG.Models;
using MTCG.Repositories;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using MTCG.Services;

internal class Program
{

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // entry point                                                                                                      //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Main entry point.</summary>
    /// <param name="args">Arguments.</param>
    /// 
    private static IServiceProvider? serviceProvider;
    static void Main(string[] args)


    {
        serviceProvider = new ServiceCollection()
         .AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>()
         .AddScoped<IUserRepository, UserRepository>()
         .AddScoped<ISessionRepository, SessionRepository>()
         .AddScoped<IPackageAndCardRepository, PackageAndCardRepository>()
         .AddScoped<IUserService, UserService>()
         .AddScoped<ISessionService, SessionService>()
         .AddScoped<IPackageAndCardService, PackageAndCardService>()
         .AddScoped<ICardService, CardService>()
         .AddScoped<ITradingService, TradingService>()
         .AddTransient<UserController>()
         .AddTransient<SessionController>()
         .AddTransient<PackageAndCardController>()
         .AddTransient<CardController>()
         .BuildServiceProvider();

        HttpSvr svr = new();
        svr.Incoming += HandleRequest;

        svr.Run();


        //Console.WriteLine(Configuration.Instance.DatabasePath);
    }


    /// <summary>Event handler for incoming server requests.</summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event arguments.</param>
    private static void HandleRequest(object sender, HttpSvrEventArgs e)
    {


        Console.WriteLine("Message: " + e.PlainMessage);
        Console.WriteLine("Header:\n");
        foreach (var header in e.Headers)
        {
            Console.WriteLine(header.Name + " : header.Value");
        }

        Console.WriteLine($"Methode: {e.Method}");
        Console.WriteLine($"Pfad: {e.Path}");


        if (e.Payload == null || e.Path == null || e.Method == null || e.Path == string.Empty || e.Method == string.Empty)
        {
            e.Reply((int)HttpCodes.BAD_REQUEST, "No data received!");
            return;
        }

        Type? controllerType = DetermineControllerType(e.Path);
        if (controllerType != null && serviceProvider != null)
        {
            var controller = serviceProvider.GetService(controllerType) as BaseController;
            controller?.HandleRequest(e);
        }
        else
            e.Reply((int)HttpCodes.BAD_REQUEST, "Bad request!");

    }

    /// <summary>Handler for picking the correct controller</summary>
    /// <param name="path">Http Path</param>

    private static Type? DetermineControllerType(string path)
    {
        if (path.StartsWith("/user"))
        {
            return typeof(UserController);
        }
        else if (path.StartsWith("/sessions"))
        {
            return typeof(SessionController);
        }
        else if (path.StartsWith("/packages") || path.StartsWith("/transactions/") || path.StartsWith("/cards"))
        {
            return typeof(PackageAndCardController);
        }



        return null;

    }





}


