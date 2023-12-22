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

using Microsoft.Extensions.DependencyInjection;
using MTCG.Services;
internal class Program
{

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // entry point                                                                                                      //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>Main entry point.</summary>
    /// <param name="args">Arguments.</param>
    /// 
    private static IServiceProvider serviceProvider;
    static void Main(string[] args)


    {
        serviceProvider = new ServiceCollection()
         .AddSingleton<IDatabaseConnectionFactory, DatabaseConnectionFactory>()
         .AddScoped<IUserRepository, UserRepository>()
         .AddScoped<ISessionRepository, SessionRepository>()
         .AddScoped<IUserService, UserService>()
         .AddScoped<ISessionService, SessionService>()// Register IUserService
         .AddTransient<UserController>()
         .AddTransient<SessionController>()
         .BuildServiceProvider();




        // Now you can use serviceProvider to get instances of your services
        var userRepository = serviceProvider.GetService<IUserRepository>();


        HttpSvr svr = new();
        svr.Incoming += _ProcessMesage;

        svr.Run();


        //Console.WriteLine(Configuration.Instance.DatabasePath);
    }


    /// <summary>Event handler for incoming server requests.</summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event arguments.</param>
    private static void _ProcessMesage(object sender, HttpSvrEventArgs e)
    {
        Console.WriteLine(e.PlainMessage);
        Console.WriteLine($"Methode: {e.Method}");
        Console.WriteLine($"Pfad: {e.Path}");
        if (e.Payload == null || e.Path == null || e.Method == null || e.Path == string.Empty || e.Method == string.Empty)

        {
            e.Reply((int)HttpCodes.BAD_REQUEST, "No data received!");

        }
        else
        {

            if (e.Path.StartsWith("/users/") && e.Method == "POST")
            {
                Console.WriteLine("users/");
            }


            if (e.Path.StartsWith("/users") && e.Method == "POST")
            {
                var userController = serviceProvider.GetService<UserController>();



                Console.WriteLine("\nplain message: " + e.PlainMessage);
                Console.WriteLine("plain message end \n");


                Console.WriteLine("\npayload message: " + e.Payload);
                Console.WriteLine("payload message end \n");

                UserCredentials? userCredentials = JsonSerializer.Deserialize<UserCredentials>(e.Payload);

                userController?.CreateUser(userCredentials, e);
            }
            if (e.Path.StartsWith("/sessions") && e.Method == "POST")
            {
                Console.WriteLine("process message richtung session");
                UserCredentials? userCredentials = JsonSerializer.Deserialize<UserCredentials>(e.Payload);
                if (userCredentials == null)
                {
                    Console.WriteLine("usercred null in sessions");
                }


                var sessionController = serviceProvider.GetService<SessionController>();

                if (sessionController == null)
                {
                    Console.WriteLine("Session controller == null ");
                }

                Console.WriteLine("direkt vor authenticate mit");
                Console.WriteLine(userCredentials.Username);
                Console.WriteLine(userCredentials.Password);
                sessionController?.AuthenticateAndCreateSession(userCredentials, e);


            }


        }
    }
}


