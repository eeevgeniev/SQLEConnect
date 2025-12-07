using SQLEConnect.Entities;
using SQLEConnect.Interfaces;
using SQLEConnect.Parsers.Base;
using SQLEConnect.Relations.Bases;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace SQLEConnect.Parsers
{
	internal class EntityParser<TModel> : BaseParser, IParser<TModel> where TModel : new()
	{
		private readonly List<TModel> _collection;
		private readonly Func<TModel, TModel, bool> _equalityComparer = (x, y) => EqualityComparer<TModel>.Default.Equals(x, y);
		private readonly Entity<TModel> _entity;

		public EntityParser(Entity<TModel> entity, Func<TModel, TModel, bool> equalityComparer)
		{
			this._collection = new List<TModel>();
			this._equalityComparer = equalityComparer;
			this._entity = entity ?? throw new ArgumentNullException();
		}

		public IEnumerable<TModel> Parse(DbDataReader dbDataReader)
		{
			base.ValidateDbDataReader(dbDataReader);

			this._entity.Wrapper = CreateWrapper();

			if (dbDataReader.Read() && dbDataReader.FieldCount > 0)
			{
				TModel model = this._entity.ParseRow(dbDataReader);

				while (dbDataReader.Read())
				{
					model = this._entity.ParseRow(dbDataReader);
				}
			}

			return this._collection;
		}

		public (bool hasResult, TModel result) ParseSingle(DbDataReader dbDataReader)
		{
			base.ValidateDbDataReader(dbDataReader);

			if (dbDataReader.Read() && dbDataReader.FieldCount > 0)
			{
				this._entity.Wrapper = CreateWrapper();

				return (true, this._entity.ParseRow(dbDataReader));
			}

			return (false, default(TModel));
		}

		public Type Type() => typeof(TModel);

		private RelationContext<TModel> CreateWrapper() => new RelationContext<TModel>(this._entity, this._collection, this._equalityComparer);

		private class RelationContext<TChild> : BaseEntityRelation<TChild> where TChild : new()
		{
			private readonly HashSet<string> _aliases;
			private readonly List<TChild> _collection;
			private readonly Func<TChild, TChild, bool> _equalityComparer;
			private readonly bool _collectionHasComparer = false;

			public RelationContext(Entity<TChild> child,
				List<TChild> collection,
				Func<TChild, TChild, bool> equalityComparer)
				: base(child)
			{
				this._aliases = new HashSet<string>();
				this._collection = collection;
				this._equalityComparer = equalityComparer;
				this._collectionHasComparer = this._equalityComparer != null;
			}

			internal override List<BaseRelation> Children => this.Child.ChildRelations;

			internal override HashSet<string> Aliases => this._aliases;

			internal override void AddAlias(string alias) => this._aliases.Add(alias);

			internal override bool DoesAliasExists(string alias) => this._aliases.Contains(alias);

			internal override TChild GetValue(TChild value) => this._equalityComparer != null ? (this._collection.FirstOrDefault(v => this._equalityComparer(v, value)) ?? value) : value;

			internal override bool IsValueAdded(TChild value) => !this._collectionHasComparer ? false : this._collection.Any(val => this._equalityComparer(value, val));

			internal override void SetValue(TChild value)
			{
				if (!this.IsValueAdded(value))
				{
					this._collection.Add(value);
				}
			}

			internal override void UpdateModel(DbDataReader rowDataReader) => base.Child.ParseRow(rowDataReader);
		}
	}
}