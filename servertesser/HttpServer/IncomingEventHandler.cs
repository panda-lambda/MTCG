using System;



namespace MTCG.HttpServer
{
    /// <summary>Implements an event handler for Incoming HTTP events.</summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">Event arguments.</param>
    public delegate void IncomingEventHandler(object sender, HttpSvrEventArgs e);
}
