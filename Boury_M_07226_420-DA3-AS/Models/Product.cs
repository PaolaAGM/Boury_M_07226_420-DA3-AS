﻿/*
 * (c) Copyright 2022 Marc-Eric Boury
 */

using Boury_M_07226_420_DA3_AS.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Boury_M_07226_420_DA3_AS.Models {
    internal class Product : IModel<Product> {

        private static readonly string DATABASE_TABLE_NAME = "dbo.Product";

        public int Id { get; private set; }
        public long GtinCode { get; set; }
        public int QtyInStock { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }


        #region Constructors


        public Product(int id) {
            this.Id = id;
        }

        public Product(string name) : this(name, 0, 0L, "") {
        }

        public Product(string name, int qtyInStock) : this(name, qtyInStock, 0L, "") {
        }

        public Product(string name, int qtyInStock, long gtinCode) : this(name, qtyInStock, gtinCode, "") {
        }

        public Product(string name, int qtyInStock, string description) : this(name, qtyInStock, 0L, description) {
        }

        public Product(string name, int qtyInStock, long gtinCode, string description) {
            this.Name = name;
            this.QtyInStock = qtyInStock;
            this.GtinCode = gtinCode;
            this.Description = description;
        }


        #endregion



        #region Static Methods



        public static Product GetById(int id) {
            Product product = new Product(id);
            return product.GetById();
        }

        public static Product GetById(int id, SqlTransaction transaction, bool withExclusiveLock = false) {
            Product product = new Product(id);
            return product.GetById(transaction, withExclusiveLock);
        }



        #endregion



        #region Methods


        public void Delete() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                this.ExecuteDeleteCommand(connection.CreateCommand());
            }
        }

        public void Delete(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            this.ExecuteDeleteCommand(cmd);
        }

        private void ExecuteDeleteCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteDeleteCommand() : Id value is 0.");
            }
            string statement = $"DELETE FROM {DATABASE_TABLE_NAME} WHERE Id = @id;";
            cmd.CommandText = statement;

            SqlParameter param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.DbType = DbType.Int32;
            param.Value = this.Id;
            cmd.Parameters.Add(param);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            if (!(affectedRows > 0)) {
                // No affected rows: no deletion occured -> row with matching Id not found
                throw new Exception($"Failed to delete {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
            }
        }



        public Product GetById() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteGetByIdCommand(connection.CreateCommand());
            }
        }

        public Product GetById(SqlTransaction transaction, bool withExclusiveLock = false) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteGetByIdCommand(cmd, withExclusiveLock);
        }

        private Product ExecuteGetByIdCommand(SqlCommand cmd, bool withExclusiveLock = false) {
            if (this.Id == 0) {
                // Id has not been set, it is initialized by default at 0;
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteGetByIdCommand() : Id value is 0.");
            }
            string statement = $"SELECT * FROM {DATABASE_TABLE_NAME} " +
                (withExclusiveLock ? "WITH  (XLOCK, ROWLOCK) " : "") +
                $"WHERE Id = @id;";
            cmd.CommandText = statement;

            SqlParameter param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.DbType = DbType.Int32;
            param.Value = this.Id;
            cmd.Parameters.Add(param);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();

                // gtinCode in the database can be NULL
                if (!reader.IsDBNull(1)) {
                    this.GtinCode = reader.GetInt64(1);
                }
                this.QtyInStock = reader.GetInt32(2);
                this.Name = reader.GetString(3);
                // gtinCode in the database can be NULL
                if (!reader.IsDBNull(4)) {
                    this.Description = reader.GetString(4);
                }

                return this;

            } else {
                throw new Exception($"No database entry for {this.GetType().FullName} with id# {this.Id}.");
            }
        }



        public Product Insert() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteInsertCommand(connection.CreateCommand());
            }
        }

        public Product Insert(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteInsertCommand(cmd);
        }

        private Product ExecuteInsertCommand(SqlCommand cmd) {
            if (this.Id > 0) {
                // Id has been set, cannot insert a product with a specific Id without risking
                // to mess up the database.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteInsertCommand() : Id value is not 0 [{this.Id}].");
            }

            // Create the INSERT statement. We do not pass any Id value since this is insertion
            // and the id is auto-generated by the database on insertion (identity).
            string statement = $"INSERT INTO {DATABASE_TABLE_NAME} (gtinCode, qtyInStock, name, description) " +
                "VALUES (@gtinCode, @qtyInStock, @name, @description); SELECT CAST(SCOPE_IDENTITY() AS int);";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter gtinCodeParam = cmd.CreateParameter();
            gtinCodeParam.ParameterName = "@gtinCode";
            gtinCodeParam.DbType = DbType.Int64;
            if (this.GtinCode == 0L) {
                gtinCodeParam.Value = DBNull.Value;
            } else {
                gtinCodeParam.Value = this.GtinCode;
            }
            cmd.Parameters.Add(gtinCodeParam);

            SqlParameter qtyInStockParam = cmd.CreateParameter();
            qtyInStockParam.ParameterName = "@qtyInStock";
            qtyInStockParam.DbType = DbType.Int32;
            qtyInStockParam.Value = this.QtyInStock;
            cmd.Parameters.Add(qtyInStockParam);

            SqlParameter nameParam = cmd.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.DbType = DbType.String;
            nameParam.Value = this.Name;
            cmd.Parameters.Add(nameParam);

            SqlParameter descriptionParam = cmd.CreateParameter();
            descriptionParam.ParameterName = "@description";
            descriptionParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.Description)) {
                descriptionParam.Value = DBNull.Value;
            } else {
                descriptionParam.Value = this.Description;
            }
            cmd.Parameters.Add(descriptionParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            this.Id = (Int32)cmd.ExecuteScalar();

            return this;

        }



        public Product Update() {
            using (SqlConnection connection = DbUtils.GetDefaultConnection()) {
                return this.ExecuteUpdateCommand(connection.CreateCommand());
            }
        }

        public Product Update(SqlTransaction transaction) {
            SqlCommand cmd = transaction.Connection.CreateCommand();
            cmd.Transaction = transaction;
            return this.ExecuteUpdateCommand(cmd);
        }

        private Product ExecuteUpdateCommand(SqlCommand cmd) {
            if (this.Id == 0) {
                // Id has not been set, cannot update a product with no specific Id to track the correct db row.
                throw new Exception($"Cannot use method {this.GetType().FullName}.ExecuteUpdateCommand() : Id value is 0.");
            }

            // Create the Update statement.
            string statement = $"UPDATE {DATABASE_TABLE_NAME} SET " +
                "gtinCode = @gtinCode, " +
                "qtyInStock = @qtyInStock, " +
                "name = @name, " +
                "description = @description " +
                "WHERE Id = @id;";
            cmd.CommandText = statement;

            // Create and add parameters
            SqlParameter whereIdParam = cmd.CreateParameter();
            whereIdParam.ParameterName = "@id";
            whereIdParam.DbType = DbType.Int32;
            whereIdParam.Value = this.Id;
            cmd.Parameters.Add(whereIdParam);

            SqlParameter gtinCodeParam = cmd.CreateParameter();
            gtinCodeParam.ParameterName = "@gtinCode";
            gtinCodeParam.DbType = DbType.Int64;
            if (this.GtinCode == 0L) {
                gtinCodeParam.Value = DBNull.Value;
            } else {
                gtinCodeParam.Value = this.GtinCode;
            }
            cmd.Parameters.Add(gtinCodeParam);

            SqlParameter qtyInStockParam = cmd.CreateParameter();
            qtyInStockParam.ParameterName = "@qtyInStock";
            qtyInStockParam.DbType = DbType.Int32;
            qtyInStockParam.Value = this.QtyInStock;
            cmd.Parameters.Add(qtyInStockParam);

            SqlParameter nameParam = cmd.CreateParameter();
            nameParam.ParameterName = "@name";
            nameParam.DbType = DbType.String;
            nameParam.Value = this.Name;
            cmd.Parameters.Add(nameParam);

            SqlParameter descriptionParam = cmd.CreateParameter();
            descriptionParam.ParameterName = "@description";
            descriptionParam.DbType = DbType.String;
            if (String.IsNullOrEmpty(this.Description)) {
                descriptionParam.Value = DBNull.Value;
            } else {
                descriptionParam.Value = this.Description;
            }
            cmd.Parameters.Add(descriptionParam);


            if (cmd.Connection.State != ConnectionState.Open) {
                cmd.Connection.Open();
            }
            int affectedRows = cmd.ExecuteNonQuery();

            // Check that a row has been updated, if not, throw exception (no row with the id
            // value found in the database, thus no update done)
            if (!(affectedRows > 0)) {
                throw new Exception($"Failed to update {this.GetType().FullName}: no database entry found for Id# {this.Id}.");
            }

            return this;

        }


        #endregion


    }
}
