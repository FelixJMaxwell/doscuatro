// Archivo: ITrabajable.cs
using UnityEngine; // Puede que no sea estrictamente necesario para la interfaz misma, pero es común incluirlo.

public interface ITrabajable
{
    // Método que se llama cuando un trabajador es añadido a la estructura.
    // Devuelve true si la adición fue exitosa, false en caso contrario (ej. si está lleno).
    bool AnadirTrabajador(PersonajeBehaviour trabajador);

    // Método que se llama cuando un trabajador es quitado de la estructura.
    void QuitarTrabajador(PersonajeBehaviour trabajador);

    // Opcional: Podrías añadir propiedades o más métodos aquí si todos los edificios trabajables los necesitaran.
    // Por ejemplo, para saber cuántos trabajadores puede albergar o cuántos tiene actualmente.
    // int MaxTrabajadores { get; }
    // int TrabajadoresActuales { get; }

    // Opcional: Si los NPCs contribuyen directamente a la producción en el edificio.
    // void ContribuirProduccion(PersonajeBehaviour trabajador);
}