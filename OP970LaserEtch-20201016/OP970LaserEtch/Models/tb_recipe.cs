using System;
using System.Linq;
using System.Text;

namespace LaserCode.Models
{
    ///<summary>
    ///
    ///</summary>
    public partial class tb_recipe
    {
           public tb_recipe(){

            id = 0;
            laser_pro = -1;
            sn_key = -1;
            recipe_name = string.Empty;
            key1_index = -1;
            key1_template = string.Empty;
            key1_data = string.Empty;
            key2_index = -1;
            key2_template = string.Empty;
            key2_data = string.Empty;
            key3_index = -1;
            key3_template = string.Empty;
            key3_data = string.Empty;
            key4_index = -1;
            key4_template = string.Empty;
            key4_data = string.Empty;
            key5_index = -1;
            key5_template = string.Empty;
            key5_data = string.Empty;
            key6_index = -1;
            key6_template = string.Empty;
            key6_data = string.Empty;
            key7_index = -1;
            key7_template = string.Empty;
            key7_data = string.Empty;
            key8_index = -1;
            key8_template = string.Empty;
            key8_data = string.Empty;
            key9_index = -1;
            key9_template = string.Empty;
            key9_data = string.Empty;
            key10_index = -1;
            key10_template = string.Empty;
            key10_data = string.Empty;
            key11_index = -1;
            key11_template = string.Empty;
            key11_data = string.Empty;
            key12_index = -1;
            key12_template = string.Empty;
            key12_data = string.Empty;
            key13_index = -1;
            key13_template = string.Empty;
            key13_data = string.Empty;
            key14_index = -1;
            key14_template = string.Empty;
            key14_data = string.Empty;
            key15_index = -1;
            key15_template = string.Empty;
            key15_data = string.Empty;
            key16_index = -1;
            key16_template = string.Empty;
            key16_data = string.Empty;
            key17_index = -1;
            key17_template = string.Empty;
            key17_data = string.Empty;
            key18_index = -1;
            key18_template = string.Empty;
            key18_data = string.Empty;
            key19_index = -1;
            key19_template = string.Empty;
            key19_data = string.Empty;
            key20_index = -1;
            key20_template = string.Empty;
            key20_data = string.Empty;

        }

        public tb_recipe(tb_recipe recipe)
        {
            id = 0;
            laser_pro = recipe.laser_pro;
            sn_key = recipe.sn_key;
            recipe_name = recipe.recipe_name;
            key1_index = recipe.key1_index;
            key1_template = recipe.key1_template;
            key1_data = recipe.key1_data;
            key2_index = recipe.key2_index;
            key2_template = recipe.key2_template;
            key2_data = recipe.key2_data;
            key3_index = recipe.key3_index;
            key3_template = recipe.key3_template;
            key3_data = recipe.key3_data;
            key4_index = recipe.key4_index;
            key4_template = recipe.key4_template;
            key4_data = recipe.key4_data;
            key5_index = recipe.key5_index;
            key5_template = recipe.key5_template;
            key5_data = recipe.key5_data;
            key6_index = recipe.key6_index;
            key6_template = recipe.key6_template;
            key6_data = recipe.key6_data;
            key7_index = recipe.key7_index;
            key7_template = recipe.key7_template;
            key7_data = recipe.key7_data;
            key8_index = recipe.key8_index;
            key8_template = recipe.key8_template;
            key8_data = recipe.key8_data;
            key9_index = recipe.key9_index;
            key9_template = recipe.key9_template;
            key9_data = recipe.key9_data;
            key10_index = recipe.key10_index;
            key10_template = recipe.key10_template;
            key10_data = recipe.key10_data;
            key11_index = recipe.key11_index;
            key11_template = recipe.key11_template;
            key11_data = recipe.key11_data;
            key12_index = recipe.key12_index;
            key12_template = recipe.key12_template;
            key12_data = recipe.key12_data;
            key13_index = recipe.key13_index;
            key13_template = recipe.key13_template;
            key13_data = recipe.key13_data;
            key14_index = recipe.key14_index;
            key14_template = recipe.key14_template;
            key14_data = recipe.key14_data;
            key15_index = recipe.key15_index;
            key15_template = recipe.key15_template;
            key15_data = recipe.key15_data;
            key16_index = recipe.key16_index;
            key16_template = recipe.key16_template;
            key16_data = recipe.key16_data;
            key17_index = recipe.key17_index;
            key17_template = recipe.key17_template;
            key17_data = recipe.key17_data;
            key18_index = recipe.key18_index;
            key18_template = recipe.key18_template;
            key18_data = recipe.key18_data;
            key19_index = recipe.key19_index;
            key19_template = recipe.key19_template;
            key19_data = recipe.key19_data;
            key20_index = recipe.key20_index;
            key20_template = recipe.key20_template;
            key20_data = recipe.key20_data;
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
           public string recipe_name {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public int laser_pro {get;set;}

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public int sn_key { get; set; }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public int key1_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key1_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key1_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key2_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key2_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key2_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key3_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key3_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key3_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key4_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key4_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key4_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key5_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key5_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key5_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key6_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key6_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key6_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key7_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key7_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key7_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key8_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key8_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key8_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key9_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key9_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key9_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key10_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key10_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key10_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key11_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key11_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key11_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key12_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key12_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key12_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key13_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key13_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key13_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key14_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key14_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key14_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key15_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key15_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key15_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key16_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key16_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key16_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key17_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key17_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key17_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key18_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key18_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key18_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key19_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key19_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key19_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int key20_index {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key20_template {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string key20_data {get;set;}

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string update_time {get;set;}

    }
}
