#include "helpers.h"
#include "stm32746g_discovery_lcd.h"
#include "stm32746g_discovery_ts.h"
#include "fonts.h"

//--//

extern "C"
{
	sFONT* BSP_LCD_GetFontBySize(int size)
	{
		switch (size)
		{
			case 8:
				return &Font8;
			case 12:
				return &Font12;
			case 16:
				return &Font16;
			case 20:
				return &Font20;
			case 24:
				return &Font24;
			default:
				return NULL;
		}
	}
	
	uint8_t BSP_TS_GetTouchInfo(uint32_t *x, uint32_t *y)
	{
		TS_StateTypeDef ts;

		BSP_TS_GetState(&ts);
				
		for (int i = 0; i < ts.touchDetected; i++)
		{
			x[i] = ts.touchX[i];
			y[i] = ts.touchY[i];
		}

		return ts.touchDetected;
	}
}