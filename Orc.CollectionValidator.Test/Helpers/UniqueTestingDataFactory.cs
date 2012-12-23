﻿namespace Orc.CollectionValidator.Test.Helpers
{
    using System.Collections.Generic;

    public class UniqueTestingDataFactory
    {
        private UniqueTestingDataFactory()
        {
        }

        public static readonly UniqueTestingDataFactory Instance = new UniqueTestingDataFactory();

        public UniqueTestingData<GenericParameter> CreateUniqueData()
        {
            var collection = new List<GenericParameter>
                                      {
                                          new GenericParameter { ID = 1, FirstName = "John", LastName = "Smith" },
                                          new GenericParameter { ID = 2, FirstName = "Sara", LastName = "Lee" },
                                          new GenericParameter { ID = 3, FirstName = "Ivan", LastName = "Susanin" },
                                          new GenericParameter { ID = 4, FirstName = "Peter", LastName = "Obama" },
                                          new GenericParameter { ID = 5, FirstName = "Serge", LastName = "Been" },
                                          new GenericParameter { ID = 6, FirstName = "Armen", LastName = "Oganesyan" },
                                      };

            return new UniqueTestingData<GenericParameter>
            {
                Collection = collection,
                ExpectedDuplicates = new int[0][]
            };
        }

        public UniqueTestingData<GenericParameter> CreateDuplicatedIdData()
        {
            var collection = new List<GenericParameter>
                                      {
                                          new GenericParameter { ID = 1, FirstName = "John", LastName = "Smith" },
                                          new GenericParameter { ID = 1, FirstName = "Sara", LastName = "Lee" },
                                          new GenericParameter { ID = 3, FirstName = "Ivan", LastName = "Susanin" },
                                          new GenericParameter { ID = 4, FirstName = "Peter", LastName = "Obama" },
                                          new GenericParameter { ID = 3, FirstName = "Serge", LastName = "Been" },
                                          new GenericParameter { ID = 5, FirstName = "Armen", LastName = "Oganesyan" },
                                      };

            return new UniqueTestingData<GenericParameter>
                       {
                           Collection = collection,
                           ExpectedDuplicates = new[] { new[] { 0, 1 }, new[] { 2, 4 } }
                       };
        }

        public UniqueTestingData<GenericParameter> CreateDuplicatedNamesData()
        {
            var collection = new List<GenericParameter>
                                      {
                                          new GenericParameter { ID = 1, FirstName = "John", LastName = "Smith" },
                                          new GenericParameter { ID = 2, FirstName = "John", LastName = "Smith" },
                                          new GenericParameter { ID = 3, FirstName = "Ivan", LastName = "Susanin" },
                                          new GenericParameter { ID = 4, FirstName = "Ivan", LastName = "Susanin" },
                                          new GenericParameter { ID = 5, FirstName = "Serge", LastName = "Been" },
                                          new GenericParameter { ID = 6, FirstName = "Ivan", LastName = "Susanin" },
                                      };

            return new UniqueTestingData<GenericParameter>
            {
                Collection = collection,
                ExpectedDuplicates = new[] { new[] { 0, 1 }, new[] { 2, 3, 5 } }
            };
        }

        public UniqueTestingData<GenericParameter> CreateDuplicatedLastNameData()
        {
            var collection = new List<GenericParameter>
                                      {
                                          new GenericParameter { ID = 1, FirstName = "John", LastName = "Smith" },
                                          new GenericParameter { ID = 2, FirstName = "Sara", LastName = "Lee" },
                                          new GenericParameter { ID = 3, FirstName = "Ivan", LastName = "Susanin" },
                                          new GenericParameter { ID = 4, FirstName = "Peter", LastName = "Susanin" },
                                          new GenericParameter { ID = 5, FirstName = "Serge", LastName = "Susanin" },
                                          new GenericParameter { ID = 6, FirstName = "Armen", LastName = "Oganesyan" },
                                      };

            return new UniqueTestingData<GenericParameter>
            {
                Collection = collection,
                ExpectedDuplicates = new[] { new[] { 2, 3, 4 } }
            };
        }

        public UniqueTestingData<string> CreateSimpleUniqueData()
        {
            return new UniqueTestingData<string>
                       {
                           Collection = new[] { "a", "b", "c", "d" },
                           ExpectedDuplicates = new int[0][]
                       };
        }

        public UniqueTestingData<string> CreateSimpleNotUniqueData()
        {
            return new UniqueTestingData<string>
            {
                Collection = new[] { "a", "b", "c", "a", "b", "b" },
                ExpectedDuplicates = new[] { new[] { 0, 3 }, new[] { 1, 4, 5 } }
            };
        }
    }
}
