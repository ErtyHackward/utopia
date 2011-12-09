// 
//  ____  _     __  __      _        _ 
// |  _ \| |__ |  \/  | ___| |_ __ _| |
// | | | | '_ \| |\/| |/ _ \ __/ _` | |
// | |_| | |_) | |  | |  __/ || (_| | |
// |____/|_.__/|_|  |_|\___|\__\__,_|_|
//
// Auto-generated from utopiamaindb on 2011-12-09 17:33:02Z. Modified by Erty Hackward
// Please visit http://code.google.com/p/dblinq2007/ for more information.
//
namespace UtopiaApi.Models
{
	using System;
	using System.ComponentModel;
	using System.Data;
	using System.Collections.Generic;
	using BLToolkit.Data.Linq;
	using BLToolkit.Data;
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
		
		public Table<User> Users
		{
			get
			{
				return this.GetTable<User>();
			}
		}
	}
	
	[TableName(Name="utopiamaindb.Users")]
	public partial class User
	{
		
		private int _id;
		
		private uint _lastIp;
		
		private System.DateTime _lastLogin;
		
		private string _login;
		
		private string _passwordHash;
		
		private System.DateTime _registerDate;
		
		public User()
		{
		}
		
		[PrimaryKey(1)]
		[DebuggerNonUserCode()]
		public int id
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
		public System.DateTime LastLogin
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
		public System.DateTime RegisterDate
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
	}
}
