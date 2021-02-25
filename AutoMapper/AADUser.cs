using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cloud.AzureAD.AutoMapper
{
    public class UserYamlModel
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public PasswordProfile PasswordProfile { get; set; }
        public IEnumerable<string> BusinessPhones { get; set; }
        public IEnumerable<string> OtherMails { get; set; }
        public IEnumerable<DirectoryObject> MemberOf { get; set; }
    }

    public class PasswordProfile
    {
        public bool? ForceChangePasswordNextSignIn { get; set; }
        public bool? ForceChangePasswordNextSignInWithMfa { get; set; }
        public string Password { get; set; }
    }

    public class DirectoryObject
    {
        public string Id { get; set; }
        public string ODataType { get; set; }
    }
}
