using SQLEConnect.Entities;
using SQLEConnect.Relations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SQLEConnect.Infrastructure;

namespace SQLEConnect
{
	public class EntityDescriptor<TModel> where TModel : new()
    {
        private readonly Entity<TModel> _entity;
        private readonly List<MemberExpression> _memberExpressions;

        public EntityDescriptor()
        {
            this._entity = new Entity<TModel>();
            this._memberExpressions = new List<MemberExpression>();
        }

		private Type TModelType => typeof(TModel);

        public EntityDescriptor<TModel> HasMember<TChild>(Expression<Func<TModel, TChild>> member) where TChild : new()
        {
			this._entity.AddMember(new Relation<TModel, TChild>(this._entity, new Entity<TChild>(), this.CreateMemberExpression(member), member.Compile()));

            return this;
		}
            
        public EntityDescriptor<TModel> HasMember<TChild>(Expression<Func<TModel, TChild>> member, EntityDescriptor<TChild> memberEntityDescriptor) where TChild : new()
        {
			this._entity.AddMember(new Relation<TModel, TChild>(this._entity, memberEntityDescriptor.GetEntity(), this.CreateMemberExpression(member), member.Compile()));

			return this;
		} 

        public EntityDescriptor<TModel> HasMemberCollection<TChild>(Expression<Func<TModel, ICollection<TChild>>> collectionMember) where TChild : new()
        {
			this.HasMemberCollection(collectionMember, null);

			return this;
		}

        public EntityDescriptor<TModel> HasMemberCollection<TChild>(Expression<Func<TModel, ICollection<TChild>>> collectionMember, EntityDescriptor<TChild> memberEntityDescriptor) where TChild : new()
        {
            Type tChildType = typeof(TChild);

			MemberExpression memberExpression = collectionMember.Body as MemberExpression;

			if (memberExpression == null)
			{
				throw new InvalidOperationException();
			}

			Type collectionType = memberExpression.Type;

			memberEntityDescriptor ??= new EntityDescriptor<TChild>();

			Entity<TChild> childEntity = memberEntityDescriptor.GetEntity();

            this._entity.AddMember(new Relation<TModel, TChild>(this._entity,
				childEntity,
                this.CreateMemberCollectionExpression(collectionMember, tChildType, collectionType),
                collectionMember?.Compile() ?? null,
                memberEntityDescriptor.EqualityComparerExpression?.Compile() ?? null));

            this.AddCollectionInitializer<TChild>(collectionMember, tChildType, collectionType);

			return this;
		}

        public EntityDescriptor<TModel> HasKey<TChild>(Expression<Func<TModel, TChild>> member)
        {
	        MemberExpression memberExpression = member.Body as MemberExpression;

	        if (memberExpression == null)
	        {
		        throw new InvalidOperationException();
	        }
	        
	        Type childType = typeof(TChild);

	        if (!childType.IsPrimitive && 
	            childType != BaseTypeHelper.StringType && 
	            childType != BaseTypeHelper.GuidType &&  
	            childType != BaseTypeHelper.ByteArrayType && 
	            childType != BaseTypeHelper.DateTimeType)
	        {
		        throw new Exception("Key must be a primitive, guid, byte array, date time or string.");
	        }
	        
	        this._memberExpressions.Add(memberExpression);
	        
	        return this;
        }

        internal Entity<TModel> GetEntity() => this._entity;

        internal Expression<Func<TModel, TModel, bool>> EqualityComparerExpression => this.GetExpression<TModel>();

        private Func<TModel, TChild, TModel> CreateMemberExpression<TChild>(Expression<Func<TModel, TChild>> member) where TChild : new()
        {
            MemberExpression memberExpression = member.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new InvalidOperationException();
            }

            ParameterExpression parameterTModelExpression = Expression.Parameter(TModelType);
            ParameterExpression parameterTChildExpression = Expression.Parameter(typeof(TChild));

            MemberExpression propExpression = Expression.PropertyOrField(parameterTModelExpression, memberExpression.Member.Name);
			LabelTarget lblTarget = Expression.Label(TModelType);
            LabelExpression lblExpression = Expression.Label(lblTarget, parameterTModelExpression);
            GotoExpression gotoExpression = Expression.Goto(lblTarget, parameterTModelExpression);

            BlockExpression blockExpression = Expression.Block(
			   Expression.Assign(propExpression, parameterTChildExpression),
				gotoExpression,
                lblExpression);

            return Expression.Lambda<Func<TModel, TChild, TModel>>(blockExpression, new ParameterExpression[] { parameterTModelExpression, parameterTChildExpression }).Compile();
        }

        private Func<TModel, TChild, TModel> CreateMemberCollectionExpression<TChild>(Expression<Func<TModel, ICollection<TChild>>> collectionMember, Type tChildType, Type collectionType) where TChild : new()
        {
            MemberExpression memberExpression = collectionMember.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new InvalidOperationException();
            }

            ParameterExpression parameterTModelExpression = Expression.Parameter(TModelType);
            ParameterExpression parameterTChildExpression = Expression.Parameter(tChildType);

            MethodInfo methodInfo = collectionType.GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, new Type[] { tChildType });

			MemberExpression propExpression = Expression.PropertyOrField(parameterTModelExpression, memberExpression.Member.Name);
			LabelTarget lblTarget = Expression.Label(TModelType);
            LabelExpression lblExpression = Expression.Label(lblTarget, parameterTModelExpression);
            GotoExpression gotoExpression = Expression.Goto(lblTarget, parameterTModelExpression);

            BlockExpression blockExpression = Expression.Block(
				Expression.Call(propExpression, methodInfo, parameterTChildExpression),
				gotoExpression,
                lblExpression);

            return Expression.Lambda<Func<TModel, TChild, TModel>>(blockExpression, new ParameterExpression[] { parameterTModelExpression, parameterTChildExpression }).Compile();
        }

        private void AddCollectionInitializer<TChild>(Expression<Func<TModel, ICollection<TChild>>> collectionMember, Type tChildType, Type collectionType)
        {
            MemberExpression memberExpression = collectionMember.Body as MemberExpression;

			if (memberExpression == null)
			{
				throw new InvalidOperationException();
			}

			ParameterExpression parameterTModelExpression = Expression.Parameter(TModelType);
            ConstructorInfo constructorInfo = !collectionType.IsInterface ? 
                collectionType.GetConstructor(new Type[] { }) : 
                typeof(HashSet<TChild>).GetConstructor(new Type[] { });

            if (constructorInfo == null)
            {
                throw new InvalidOperationException();
            }

			MemberExpression propExpression = Expression.PropertyOrField(parameterTModelExpression, memberExpression.Member.Name);
			LabelTarget lblTarget = Expression.Label(TModelType);

            this._entity.AddInitializer(Expression.Lambda<Func<TModel, TModel>>(
                Expression.Block(
                    Expression.IfThen(
                        Expression.Equal(propExpression, Expression.Constant(null)),
						Expression.Assign(propExpression, Expression.New(constructorInfo))),
						Expression.Goto(lblTarget, parameterTModelExpression),
                        Expression.Label(lblTarget, parameterTModelExpression)
                    ), new ParameterExpression[] { parameterTModelExpression }).Compile()
                );
        }

        private Expression<Func<TMember, TMember, bool>> GetExpression<TMember>()
        {
	        if (this._memberExpressions.Count == 0)
	        {
		        return null;
	        }
	        
	        ParameterExpression parameterXExpression = Expression.Parameter(typeof(TMember)); 
	        ParameterExpression parameterYExpression = Expression.Parameter(typeof(TMember));

	        BlockExpression blockExpression = null;
	        
	        if (this._memberExpressions.Count == 1)
	        {
		        blockExpression = Expression.Block(
			        Expression.Equal(
				        Expression.PropertyOrField(parameterXExpression, this._memberExpressions[0].Member.Name),
				        Expression.PropertyOrField(parameterYExpression, this._memberExpressions[0].Member.Name)));
	        }
	        else
	        {
		        blockExpression = Expression.Block(this._memberExpressions.Aggregate<MemberExpression, BinaryExpression>(null,
			        (BinaryExpression acc, MemberExpression current) => acc != null
				        ? Expression.AndAlso(acc, Expression.Equal(
					        Expression.PropertyOrField(parameterXExpression, current.Member.Name),
					        Expression.PropertyOrField(parameterYExpression, current.Member.Name)))
				        : Expression.Equal(
					        Expression.PropertyOrField(parameterXExpression, current.Member.Name),
					        Expression.PropertyOrField(parameterYExpression, current.Member.Name))));
	        }
	        
	        return Expression.Lambda<Func<TMember, TMember, bool>>(
		        blockExpression, 
		        new ParameterExpression[] { parameterXExpression, parameterYExpression });
        }
    }
}
