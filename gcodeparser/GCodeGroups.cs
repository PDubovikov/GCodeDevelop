/*
 * Создано в SharpDevelop.
 * Пользователь: P.Dubovikov
 * Дата: 30.09.2016
 * Время: 10:47
 * 
 * Для изменения этого шаблона используйте меню "Инструменты | Параметры | Кодирование | Стандартные заголовки".
 */

using System ;

namespace gcodeparser.gcodes 
{
	
	public enum GCodeGroups {

    ActivePlane,
    AxisOffset,
    CollantMode,
    CoordinateSystemMode,
    CutterLengthCompMode,
    CutterRadiusCompMode,
    DistanceMode,
    FeedRateMode,
    MotionsModes,
    PathControleMode,
    PredefinedPosition,
    ReferenceLocation,
    RetrackMode,
    SFOverrideMode,
    SpindleMode,
    StopMode,
    Units,
    Default	
    	
	};
}