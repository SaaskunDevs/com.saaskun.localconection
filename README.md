>[!NOTE]
>Para mandar datos se tiene que mandar un codigo y luego el dato. Para dividir se tiene que usar el simbolo |
>Ejemplo: PositionX|12.215

>[!IMPORTANT]
>Necesitas del package de ThreadThispacher primero
>https://github.com/SaaskunDevs/com.saaskunstudios.threaddispatcher.git

# com.saaskunstudios.localconection
 Transferencia de datos de manera local
 Para mensajes simples de datos en donde no importa tanto si lleguen o no se pued usar UDP, por lo general siempre llegan los mensajes, pero es recomendado mandar mensajes con bajo peso como texto.
 Para mensajes como datos de voz, imagenes, etc se recomienda usar TCP para que no se tenga problemas en el envio de datos.
