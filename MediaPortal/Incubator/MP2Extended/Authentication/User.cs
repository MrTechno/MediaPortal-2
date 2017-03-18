using System;
using MediaPortal.Plugins.MP2Extended.Settings;

namespace MediaPortal.Plugins.MP2Extended.Authentication
{
  class User
  {
    public Guid Id;
    public string UserName;
    public UserTypes Type;

    public User(Guid id, string userName, UserTypes userType)
    {
      Id = id;
      UserName = userName;
      Type = userType;
    }
  }
}
