using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSugar;
using LaserCode.Utility;

namespace LaserCode.BLL
{
    public abstract class BLLBase<T> where T:class,new()
    {
        public enum ExecuteResult
        {
            Exec_Success = 1,
            Exec_Fail = 2,
            Exec_Exception=3,
        }
        //db curd option

        /// <summary>
        /// 单数据插入
        /// </summary>
        /// <param name="model">实体</param>
        /// <returns></returns>
        public static ExecuteResult InsertData(T model)
        {
            try
            {
                return SugarBase.DB.Insertable(model).ExecuteCommand() > 0 ? ExecuteResult.Exec_Success : ExecuteResult.Exec_Fail;
            }
            catch (Exception ex)
            {
                MyLog.WriteMsg(MyLog.MessageType.EXCEPTION, "", "InsertData", "", ex);
                return ExecuteResult.Exec_Exception;
            }
        }
        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="models">批量实体</param>
        /// <returns></returns>
        public static ExecuteResult InsertBatchData(IList<T> models)
        {
            try
            {
                return SugarBase.DB.Insertable(models.ToArray()).ExecuteCommand() > 0 ? ExecuteResult.Exec_Success : ExecuteResult.Exec_Fail;
            }
            catch (Exception ex)
            {
                MyLog.WriteMsg(MyLog.MessageType.EXCEPTION, "", "InsertBatchData", "", ex);
                return ExecuteResult.Exec_Exception;
            }
        }
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="model">实体</param>
        /// <returns></returns>
        public static ExecuteResult UpdateDataByModel(T model)
        {
            try
            {
                return SugarBase.DB.Updateable<T>(model).ExecuteCommand() > 0?ExecuteResult.Exec_Success : ExecuteResult.Exec_Fail;
            }
            catch (Exception ex)
            {
                MyLog.WriteMsg(MyLog.MessageType.EXCEPTION, "", "UpdateDataByModel", "", ex);
                return ExecuteResult.Exec_Exception;
            }
        }

        
        /// <summary>
        /// 根据实体删除数据
        /// </summary>
        /// <param name="model">实体</param>
        /// <returns></returns>
        public static ExecuteResult DeleteDataByModel(T model)
        {
            try
            {
                return SugarBase.DB.Deleteable<T>(model).ExecuteCommand() > 0 ? ExecuteResult.Exec_Success : ExecuteResult.Exec_Fail;
            }
            catch (Exception ex)
            {
                MyLog.WriteMsg(MyLog.MessageType.EXCEPTION, "", "DeleteData", "", ex);
                return ExecuteResult.Exec_Exception;
            }
        }
        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="nID">主键</param>
        /// <returns></returns>
        public static ExecuteResult DeleteDataByPK(int nID)
        {
            try
            {
                return SugarBase.DB.Deleteable<T>().In(nID).ExecuteCommand() > 0 ? ExecuteResult.Exec_Success : ExecuteResult.Exec_Fail;
            }
            catch (Exception ex)
            {
                MyLog.WriteMsg(MyLog.MessageType.EXCEPTION,"", "DeleteDataByPK", "", ex);
                return ExecuteResult.Exec_Exception;
            }
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public static IList<T> SelectData()
        {
            try
            {
                return SugarBase.DB.Queryable<T>().ToList();
            }
            catch (Exception ex)
            {
                MyLog.WriteMsg(MyLog.MessageType.EXCEPTION, "", "SelectData", "", ex);
                return new List<T>();
            }
        }

        /// <summary>
        /// 获取单条数据
        /// </summary>
        /// <param name="nID">主键ID</param>
        /// <returns></returns>
        public static ExecuteResult GetSingleByPK(int nID,ref T data)
        {
            try
            {
                data= SugarBase.DB.Queryable<T>().InSingle(nID);
                return data!=null?ExecuteResult.Exec_Success:ExecuteResult.Exec_Fail;
            }
            catch (Exception ex)
            {
                MyLog.WriteMsg(MyLog.MessageType.EXCEPTION, "", "GetSingleByPK", "", ex);
                data = new T();
                return ExecuteResult.Exec_Success;
            }
        }
        
    }
}
