#pragma once
#include "Operator.h"
using namespace System;
using namespace V5::Query;

namespace V5
{
	namespace Collections
	{
		public ref class IndexSet
		{
		private:
			UInt32 offset;
			UInt32 length;
			array<UInt64>^ bitVector;

		public:
			IndexSet();
			IndexSet(UInt32 offset, UInt32 length);

			// Get/Set bits and see Count set
			property Boolean default[Int32] { bool get(Int32 index); void set(Int32 index, Boolean value); }
			property Int32 Count { Int32 get(); }

			virtual Boolean Equals(Object^ other) override;

			// Set to None/All quickly
			IndexSet^ None();
			IndexSet^ All();

			// Set operations
			IndexSet^ And(IndexSet^ other);
			IndexSet^ AndNot(IndexSet^ other);
			IndexSet^ Or(IndexSet^ other);

			// Where [extension method]?
			generic <typename T>
			IndexSet^ And(array<T>^ values, Operator op, T value);
		};
	}
}
