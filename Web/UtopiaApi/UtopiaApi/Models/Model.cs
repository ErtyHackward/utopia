// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from utopiamaindb on 2011-12-15 10:58:19Z. Modified by Erty Hackward
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//
namespace UtopiaApi.Models
{
    using System.Collections.Generic;
	using BLToolkit.Data.Linq;
    using BLToolkit.DataAccess;
	using BLToolkit.Mapping;
	using System.Diagnostics;
	
	
	public partial class UtopiaDataContext : DataContext
	{
		
		#region Extensibility Method Declarations
		partial void OnCreated();
		#endregion
		
		
		public UtopiaDataContext()
		{
		}

        public UtopiaDataContext(string connectionString) : 
				base(connectionString)
		{
			this.OnCreated();
		}
		
		public Table<Culture> Cultures
		{
			get
			{
				return this.GetTable<Culture>();
			}
		}
		
		public Table<Server> Servers
		{
			get
			{
				return this.GetTable<Server>();
			}
		}
		
		public Table<Token> Tokens
		{
			get
			{
				return this.GetTable<Token>();
			}
		}
		
		public Table<User> Users
		{
			get
			{
				return this.GetTable<User>();
			}
		}
	}
	
	[TableName(Name="utopiamaindb.Cultures")]
	public partial class Culture
	{
		
		private uint _id;
		
		private string _name;
		
		private List<Server> _servers;
		
		private List<User> _users;
		
		public Culture()
		{
		}
		
		[PrimaryKey(1)]
		[Identity()]
		[DebuggerNonUserCode()]
		public uint Id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}
		
		#region Children
		[Association(Storage="_servers", OtherKey="Culture", ThisKey="Id")]
		[DebuggerNonUserCode()]
		public List<Server> Servers
		{
			get
			{
				return this._servers;
			}
			set
			{
				this._servers = value;
			}
		}
		
		[Association(Storage="_users", OtherKey="Culture", ThisKey="Id")]
		[DebuggerNonUserCode()]
		public List<User> Users
		{
			get
			{
				return this._users;
			}
			set
			{
				this._users = value;
			}
		}
		#endregion
	}
	
	[TableName(Name="utopiamaindb.Servers")]
	public partial class Server
	{
		
		private string _address;
		
		private uint _culture;
		
		private uint _id;
		
		private System.DateTime _lastUpdate;
		
		private string _name;
		
		private uint _usersCount;
		
		private Culture _cultureCulture;
		
		public Server()
		{
		}
		
		[DebuggerNonUserCode()]
		public string Address
		{
			get
			{
				return this._address;
			}
			set
			{
				this._address = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public uint Culture
		{
			get
			{
				return this._culture;
			}
			set
			{
				this._culture = value;
			}
		}
		
		[PrimaryKey(1)]
		[Identity()]
		[DebuggerNonUserCode()]
		public uint Id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public System.DateTime LastUpdate
		{
			get
			{
				return this._lastUpdate;
			}
			set
			{
				this._lastUpdate = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public uint UsersCount
		{
			get
			{
				return this._usersCount;
			}
			set
			{
				this._usersCount = value;
			}
		}
		
		#region Parents
		[Association(Storage="_cultureCulture", OtherKey="Id", ThisKey="Culture")]
		[DebuggerNonUserCode()]
		public Culture CultureCulture
		{
			get
			{
				return this._cultureCulture;
			}
			set
			{
				this._cultureCulture = value;
			}
		}
		#endregion
	}
	
	[TableName(Name="utopiamaindb.Tokens")]
	public partial class Token
	{
		
		private System.DateTime _lastUpdate;
		
		private string _tokenValue;
		
		private uint _userId;
		
		private User _user;
		
		public Token()
		{
		}
		
		[DebuggerNonUserCode()]
		public System.DateTime LastUpdate
		{
			get
			{
				return this._lastUpdate;
			}
			set
			{
				this._lastUpdate = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public string TokenValue
		{
			get
			{
				return this._tokenValue;
			}
			set
			{
				this._tokenValue = value;
			}
		}
		
		[PrimaryKey(1)]
		[DebuggerNonUserCode()]
		public uint UserId
		{
			get
			{
				return this._userId;
			}
			set
			{
				this._userId = value;
			}
		}
		
		#region Parents
		[Association(Storage="_user", OtherKey="id", ThisKey="UserId")]
		[DebuggerNonUserCode()]
		public User User
		{
			get
			{
				return this._user;
			}
			set
			{
				this._user = value;
			}
		}
		#endregion
	}
	
	[TableName(Name="utopiamaindb.Users")]
	public partial class User
	{
		
		private byte _confirmed;
		
		private string _confirmToken;
		
		private uint _culture;
		
		private string _displayName;
		
		private uint _id;
		
		private uint _lastIp;
		
		private System.Nullable<System.DateTime> _lastLogin;
		
		private string _login;
		
		private string _passwordHash;
		
		private System.Nullable<System.DateTime> _registerDate;
		
		private List<Token> _tokens;
		
		private Culture _cultureCulture;
		
		public User()
		{
		}
		
		[DebuggerNonUserCode()]
		public byte Confirmed
		{
			get
			{
				return this._confirmed;
			}
			set
			{
				this._confirmed = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public string ConfirmToken
		{
			get
			{
				return this._confirmToken;
			}
			set
			{
				this._confirmToken = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public uint Culture
		{
			get
			{
				return this._culture;
			}
			set
			{
				this._culture = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public string DisplayName
		{
			get
			{
				return this._displayName;
			}
			set
			{
				this._displayName = value;
			}
		}
		
		[PrimaryKey(1)]
		[Identity()]
		[DebuggerNonUserCode()]
		public uint id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public uint LastIp
		{
			get
			{
				return this._lastIp;
			}
			set
			{
				this._lastIp = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public System.Nullable<System.DateTime> LastLogin
		{
			get
			{
				return this._lastLogin;
			}
			set
			{
				this._lastLogin = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public string Login
		{
			get
			{
				return this._login;
			}
			set
			{
				this._login = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public string PasswordHash
		{
			get
			{
				return this._passwordHash;
			}
			set
			{
				this._passwordHash = value;
			}
		}
		
		[DebuggerNonUserCode()]
		public System.Nullable<System.DateTime> RegisterDate
		{
			get
			{
				return this._registerDate;
			}
			set
			{
				this._registerDate = value;
			}
		}
		
		#region Children
		[Association(Storage="_tokens", OtherKey="UserId", ThisKey="id")]
		[DebuggerNonUserCode()]
		public List<Token> Tokens
		{
			get
			{
				return this._tokens;
			}
			set
			{
				this._tokens = value;
			}
		}
		#endregion
		
		#region Parents
		[Association(Storage="_cultureCulture", OtherKey="Id", ThisKey="Culture")]
		[DebuggerNonUserCode()]
		public Culture CultureCulture
		{
			get
			{
				return this._cultureCulture;
			}
			set
			{
				this._cultureCulture = value;
			}
		}
		#endregion
	}
}
