namespace Mensajes.Comunes
{
    public static class GeneradorFallas
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Simula un fallo con una probabilidad determinada.
        /// </summary>
        /// <param name="porcentaje">Porcentaje de probabilidad de fallo (0-100).</param>
        public static void SimularFalloAleatorio(double porcentaje)
        {
            if (porcentaje < 0 || porcentaje > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(porcentaje), "El porcentaje debe estar entre 0 y 100.");
            }

            double valorAleatorio = _random.NextDouble() * 100; 

            if (valorAleatorio < porcentaje)
            {
                throw new ExcepcionServicio("Se generó una excepción inducida aleatoriamente.");
            }

        }
    }

}
