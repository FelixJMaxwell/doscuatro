using UnityEngine;

public class BuildingType
{
    public enum Estructura{
        //Vivienda
        CasaBasica, //lugar donde viviran las personas
        CasaAvanzada,
        CasaComunal,

        //Investigación y Educación
        Biblioteca, //lugar donde se investigaran tecnologicas disponibles del arbol tecnologico
        CampoEntrenamientoEscuela, // entrenamiento o educacion de personas
        SalaDeRituales, // se utilizara para añadir "upgrades" a las personas
        SalaDeConocimientoProhibido, // Rituales peligrosos mucha ganancia (buff) - mucha perdida (debuff)

        //Produccion de recursos basicos
        Granja, //lugar donde se hara comida
        Pozo, //extracción de agua para los personajes o cualquier otro uso
        Mina, //produccion de materiales en crudo
        Establo, // produccion animal (piel, carne)
        Cocina, //Procesamiento animal

        //Produccion de bienes manufacturados
        Forja, // se utilizara para construir o crear diferentes equipos para las personas
        Carpintero, //produccion de bienes
        Panaderia, //produccion de alimento
        Sastreria, //produccion de bienes para vestir a las personas
        Alquimista, //produccion de medicinas o brebajes para hospitales o individuos

        //Defensa
        Muralla, //Proteccion alrededor de la aldea de peligros externos

        //Culto / Religion
        Obelisco, //version pequeña del monolito que afectara de diferentes formas lo edificios alrededor
        AltarDeSacrificio, // lugar donde se pueden sacrificar personas o colocar cadaveres para ofrendas
        CentroConversion, // se utilizara para adoctrinar personas externas al culto
        CapillaMenor,

        //Gestión y Almacenaje
        Almacen, // resguardo central de bienes
        Mercado, //  lugar central para intercambio de bienes
        Ayuntamiento, // Edificio para consulta de estadistica
        Refrigerador,

        //Salud
        Hospital, // cuidado de personas del culto y cura de enfermedades

        //Especializacion
        JardinCeremonial, //se utilizara para desarrollar derivados de la botanica
    }
}
