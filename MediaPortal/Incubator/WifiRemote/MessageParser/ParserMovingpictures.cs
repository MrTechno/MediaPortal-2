﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Deusty.Net;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Common.SystemCommunication;
using MediaPortal.UiComponents.Media.Models;
using MediaPortal.UI.Presentation.Players;
using MediaPortal.UI.ServerCommunication;
using Newtonsoft.Json.Linq;

namespace MediaPortal.Plugins.WifiRemote.MessageParser
{
  internal class ParserMovingpictures
  {
    public static bool Parse(JObject message, SocketServer server, AsyncSocket sender)
    {
      string action = (string)message["Action"];

      if (!string.IsNullOrEmpty(action))
      {
        // TODO: implement
        // Show movie details for this movie
        if (action == "moviedetails")
        {
          //MediaNavigationModel.GetCurrentInstance().NavigationData.AvailableScreens.First().
          /*string movieName = (string)message["MovieName"];
          if (!string.IsNullOrEmpty(movieName))
          {
            int movieId = MovingPicturesHelper.GetMovieIdByName(movieName);
            MovingPicturesHelper.ShowMovieDetails(movieId);
          }*/
        }
        // Play a movie with MovingPictures
        else if (action == "playmovie")
        {
          // we use the FileHandler as MediaItem id
          string fileType = (string)message["FileType"];
          string filePath = (string)message["Filepath"];
          string id = (string)message["MovieId"];
          int startPos = (message["StartPosition"] != null) ? (int)message["StartPosition"] : 0;

          ServiceRegistration.Get<ILogger>().Debug("PlayFile: fileType: {0}, filePath: {1}, FileHandler/id: {2}, StartPos: {3}", fileType, filePath, id, startPos);

          Guid mediaItemGuid;
          if (!Guid.TryParse(id, out mediaItemGuid))
          {
            ServiceRegistration.Get<ILogger>().Info("PlayFile: Couldn't convert fileHandler '{0} to Guid", id);
            return false;
          }

          Helper.PlayMediaItem(mediaItemGuid, startPos);
        }
      }

      return true;
    }
  }
}
