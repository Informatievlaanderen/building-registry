namespace BuildingRegistry.Building
{
    using System;

    public struct BuildingUnitFunction
    {
        public static readonly BuildingUnitFunction Common = new BuildingUnitFunction("Common");
        public static readonly BuildingUnitFunction Unknown = new BuildingUnitFunction("Unknown");

        public string Function { get; }

        private BuildingUnitFunction(string function) => Function = function;

        public static BuildingUnitFunction Parse(string function)
        {
            if (function != Common.Function &&
                function != Unknown.Function)
                throw new NotImplementedException($"Cannot parse {function} to BuildingUnitFunction");

            return new BuildingUnitFunction(function);
        }

        public static implicit operator string(BuildingUnitFunction function) => function.Function;
    }
}
