namespace Wave.Platform.Messaging
{
    public enum UserManagerMessageID : short 
    {
		UserLogin = 29,
		SuccessfulLogin = 30,
		UserAccountLockedOut = 31,
		UserAccountInactive = 32,
		UserAlreadyLoggedIn = 33,
		IncorrectLoginDetails = 34,
		UserLogout = 35,
		UserNotAuthenticated = 38,
		UserAccountAccessDenied = 40,
		DisplayMessage = 41,
		
		Challenge = 42,
		EncryptionKeys = 43,

        UserAccountCreation = 49,
		Login = 50,
		LoginResponse = 51
    }
    
    public enum UserManagerFieldID : short
    {
        Message = 4,
        Password = 7,
        LanguageID = 16,
		LoginInfo = 47,
		FailureReasonID = 48,
		Challenge = 49,
		UserCredentials = 51,
        SessionKey = 52,
        GlobalServerKey = 53,
        PriorityMask = 54,
        SessionInfo = 58,
		DeviceBuildID = 65,
        CreatedAccountUserName = 66,
        CreatedAccountPasswordHash = 67,
        CSLVersion = 69,
        DoNotEncrypt = 70,
        Timezone = 71,
        UseUriShortForm = 72,
        AlphaSupport = 73,
        MaxWeMessageVersion = 74,
        CompressionSupport = 75,

        EncryptSession = 80,
        DeviceSettings = 81,
        ApplicationRequest = 82,
        LoginStatus = 84,
        CreateAccount = 85,
        DeviceInfo = 87,
        ApplicationContext = 320,
        ApplicationUnqualifiedURI = 321,
        ApplicationFullyQualifiedURI = 322
    }

    public enum UserLoginStatus : short
    {
        Success = 0,
        FailedInvalidCredentials = 1,
        FailedNoUser = 2
    }
}
