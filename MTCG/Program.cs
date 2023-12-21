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
         .AddScoped<IUserService, UserService>()  // Register IUserService
         .AddTransient<UserController>()
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

        if (e.Path.StartsWith("/users/") && e.Method == "POST")
        {
            Console.WriteLine("users/");
        }


        if (e.Path.StartsWith("/users") && e.Method == "POST")
        {
            var userController = serviceProvider.GetService<UserController>();

            if (e.Payload != null)
            {

                Console.WriteLine("\nplain message: "+ e.PlainMessage);
                Console.WriteLine("plain message end \n");


                Console.WriteLine("\npayload message: " + e.Payload);
                Console.WriteLine("payload message end \n");




                UserCredentials userCredentials = JsonSerializer.Deserialize<UserCredentials>(e.Payload);

                //TODO: check if userCredentials is null
                //TODO: check if userCredentials.Username is null
                //TODO: check if userCredentials.Password is null
                userController.CreateUser(userCredentials, e);
            }


        }


        e.Reply(200, e.PlainMessage);
    }
}


