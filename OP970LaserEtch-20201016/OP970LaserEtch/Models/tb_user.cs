using System;
using System.Linq;
using System.Text;

namespace LaserCode.Models
{
    ///<summary>
    ///
    ///</summary>
    public partial class tb_user
    {
        public tb_user()
        {
            id = 0;
            user = "";
            password = "";
            level = "";
            update_time = DateTime.Now.ToString("yyyy-MM-dd");
        }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
        public int id {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
        public string user {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
        public string password {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
        public string level {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
        public string update_time {get;set;}


    }
}
