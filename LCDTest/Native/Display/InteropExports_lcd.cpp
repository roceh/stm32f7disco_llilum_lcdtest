#include "helpers.h"

extern "C"
{	
	static LTDC_HandleTypeDef  hLtdcHandler;
	
	/* Display enable pin */
	#define LCD_DISP_PIN                    GPIO_PIN_12
	#define LCD_DISP_GPIO_PORT              GPIOI
	#define LCD_DISP_GPIO_CLK_ENABLE()      __HAL_RCC_GPIOI_CLK_ENABLE()
	#define LCD_DISP_GPIO_CLK_DISABLE()     __HAL_RCC_GPIOI_CLK_DISABLE()

	/* Backlight control pin */
	#define LCD_BL_CTRL_PIN                  GPIO_PIN_3
	#define LCD_BL_CTRL_GPIO_PORT            GPIOK
	#define LCD_BL_CTRL_GPIO_CLK_ENABLE()    __HAL_RCC_GPIOK_CLK_ENABLE()
	#define LCD_BL_CTRL_GPIO_CLK_DISABLE()   __HAL_RCC_GPIOK_CLK_DISABLE()

	#define  RK043FN48H_WIDTH			 ((uint16_t)480)  /* LCD PIXEL WIDTH            */
	#define  RK043FN48H_HEIGHT			 ((uint16_t)272)  /* LCD PIXEL HEIGHT           */
	#define  RK043FN48H_HSYNC            ((uint16_t)41)   /* Horizontal synchronization */
	#define  RK043FN48H_HBP              ((uint16_t)13)   /* Horizontal back porch      */
	#define  RK043FN48H_HFP              ((uint16_t)32)   /* Horizontal front porch     */
	#define  RK043FN48H_VSYNC            ((uint16_t)10)   /* Vertical synchronization   */
	#define  RK043FN48H_VBP              ((uint16_t)2)    /* Vertical back porch        */
	#define  RK043FN48H_VFP              ((uint16_t)2)    /* Vertical front porch       */
	#define  RK043FN48H_FREQUENCY_DIVIDER    5            /* LCD Frequency divider      */

	static uint32_t* DrawAddress;

	void LCD_Init(uint32_t* activeAddress, uint32_t* drawAddress)
	{
		/* Timing Configuration */
		hLtdcHandler.Init.HorizontalSync = (RK043FN48H_HSYNC - 1);
		hLtdcHandler.Init.VerticalSync = (RK043FN48H_VSYNC - 1);
		hLtdcHandler.Init.AccumulatedHBP = (RK043FN48H_HSYNC + RK043FN48H_HBP - 1);
		hLtdcHandler.Init.AccumulatedVBP = (RK043FN48H_VSYNC + RK043FN48H_VBP - 1);
		hLtdcHandler.Init.AccumulatedActiveH = (RK043FN48H_HEIGHT + RK043FN48H_VSYNC + RK043FN48H_VBP - 1);
		hLtdcHandler.Init.AccumulatedActiveW = (RK043FN48H_WIDTH + RK043FN48H_HSYNC + RK043FN48H_HBP - 1);
		hLtdcHandler.Init.TotalHeigh = (RK043FN48H_HEIGHT + RK043FN48H_VSYNC + RK043FN48H_VBP + RK043FN48H_VFP - 1);
		hLtdcHandler.Init.TotalWidth = (RK043FN48H_WIDTH + RK043FN48H_HSYNC + RK043FN48H_HBP + RK043FN48H_HFP - 1);

		static RCC_PeriphCLKInitTypeDef  periph_clk_init_struct;

		/* RK043FN48H LCD clock configuration */
		/* PLLSAI_VCO Input = HSE_VALUE/PLL_M = 1 Mhz */
		/* PLLSAI_VCO Output = PLLSAI_VCO Input * PLLSAIN = 192 Mhz */
		/* PLLLCDCLK = PLLSAI_VCO Output/PLLSAIR = 192/5 = 38.4 Mhz */
		/* LTDC clock frequency = PLLLCDCLK / LTDC_PLLSAI_DIVR_4 = 38.4/4 = 9.6Mhz */
		periph_clk_init_struct.PeriphClockSelection = RCC_PERIPHCLK_LTDC;
		periph_clk_init_struct.PLLSAI.PLLSAIN = 192;
		periph_clk_init_struct.PLLSAI.PLLSAIR = RK043FN48H_FREQUENCY_DIVIDER;
		periph_clk_init_struct.PLLSAIDivR = RCC_PLLSAIDIVR_4;
		HAL_RCCEx_PeriphCLKConfig(&periph_clk_init_struct);	

		/* Initialize the LCD pixel width and pixel height */
		hLtdcHandler.LayerCfg->ImageWidth = RK043FN48H_WIDTH;
		hLtdcHandler.LayerCfg->ImageHeight = RK043FN48H_HEIGHT;

		/* Background value */
		hLtdcHandler.Init.Backcolor.Blue = 0;
		hLtdcHandler.Init.Backcolor.Green = 0;
		hLtdcHandler.Init.Backcolor.Red = 0;

		/* Polarity */
		hLtdcHandler.Init.HSPolarity = LTDC_HSPOLARITY_AL;
		hLtdcHandler.Init.VSPolarity = LTDC_VSPOLARITY_AL;
		hLtdcHandler.Init.DEPolarity = LTDC_DEPOLARITY_AL;
		hLtdcHandler.Init.PCPolarity = LTDC_PCPOLARITY_IPC;
		hLtdcHandler.Instance = LTDC;

		if (HAL_LTDC_GetState(&hLtdcHandler) == HAL_LTDC_STATE_RESET)
		{
			GPIO_InitTypeDef gpio_init_structure;

			/* Enable the LTDC and DMA2D clocks */
			__HAL_RCC_LTDC_CLK_ENABLE();
			__HAL_RCC_DMA2D_CLK_ENABLE();

			/* Enable GPIOs clock */
			__HAL_RCC_GPIOE_CLK_ENABLE();
			__HAL_RCC_GPIOG_CLK_ENABLE();
			__HAL_RCC_GPIOI_CLK_ENABLE();
			__HAL_RCC_GPIOJ_CLK_ENABLE();
			__HAL_RCC_GPIOK_CLK_ENABLE();
			LCD_DISP_GPIO_CLK_ENABLE();
			LCD_BL_CTRL_GPIO_CLK_ENABLE();

			/*** LTDC Pins configuration ***/
			/* GPIOE configuration */
			gpio_init_structure.Pin = GPIO_PIN_4;
			gpio_init_structure.Mode = GPIO_MODE_AF_PP;
			gpio_init_structure.Pull = GPIO_NOPULL;
			gpio_init_structure.Speed = GPIO_SPEED_FAST;
			gpio_init_structure.Alternate = GPIO_AF14_LTDC;
			HAL_GPIO_Init(GPIOE, &gpio_init_structure);

			/* GPIOG configuration */
			gpio_init_structure.Pin = GPIO_PIN_12;
			gpio_init_structure.Mode = GPIO_MODE_AF_PP;
			gpio_init_structure.Alternate = GPIO_AF9_LTDC;
			HAL_GPIO_Init(GPIOG, &gpio_init_structure);

			/* GPIOI LTDC alternate configuration */
			gpio_init_structure.Pin = GPIO_PIN_8 | GPIO_PIN_9 | GPIO_PIN_10 | \
				GPIO_PIN_13 | GPIO_PIN_14 | GPIO_PIN_15;
			gpio_init_structure.Mode = GPIO_MODE_AF_PP;
			gpio_init_structure.Alternate = GPIO_AF14_LTDC;
			HAL_GPIO_Init(GPIOI, &gpio_init_structure);

			/* GPIOJ configuration */
			gpio_init_structure.Pin = GPIO_PIN_0 | GPIO_PIN_1 | GPIO_PIN_2 | GPIO_PIN_3 | \
				GPIO_PIN_4 | GPIO_PIN_5 | GPIO_PIN_6 | GPIO_PIN_7 | \
				GPIO_PIN_8 | GPIO_PIN_9 | GPIO_PIN_10 | GPIO_PIN_11 | \
				GPIO_PIN_13 | GPIO_PIN_14 | GPIO_PIN_15;
			gpio_init_structure.Mode = GPIO_MODE_AF_PP;
			gpio_init_structure.Alternate = GPIO_AF14_LTDC;
			HAL_GPIO_Init(GPIOJ, &gpio_init_structure);

			/* GPIOK configuration */
			gpio_init_structure.Pin = GPIO_PIN_0 | GPIO_PIN_1 | GPIO_PIN_2 | GPIO_PIN_4 | \
				GPIO_PIN_5 | GPIO_PIN_6 | GPIO_PIN_7;
			gpio_init_structure.Mode = GPIO_MODE_AF_PP;
			gpio_init_structure.Alternate = GPIO_AF14_LTDC;
			HAL_GPIO_Init(GPIOK, &gpio_init_structure);

			/* LCD_DISP GPIO configuration */
			gpio_init_structure.Pin = LCD_DISP_PIN;     /* LCD_DISP pin has to be manually controlled */
			gpio_init_structure.Mode = GPIO_MODE_OUTPUT_PP;
			HAL_GPIO_Init(LCD_DISP_GPIO_PORT, &gpio_init_structure);

			/* LCD_BL_CTRL GPIO configuration */
			gpio_init_structure.Pin = LCD_BL_CTRL_PIN;  /* LCD_BL_CTRL pin has to be manually controlled */
			gpio_init_structure.Mode = GPIO_MODE_OUTPUT_PP;
			HAL_GPIO_Init(LCD_BL_CTRL_GPIO_PORT, &gpio_init_structure);
		}

		HAL_LTDC_Init(&hLtdcHandler);

		/* Assert display enable LCD_DISP pin */
		HAL_GPIO_WritePin(LCD_DISP_GPIO_PORT, LCD_DISP_PIN, GPIO_PIN_SET);

		/* Assert backlight LCD_BL_CTRL pin */
		HAL_GPIO_WritePin(LCD_BL_CTRL_GPIO_PORT, LCD_BL_CTRL_PIN, GPIO_PIN_SET);
		
		LTDC_LayerCfgTypeDef  layer_cfg;

		/* Layer Init */
		layer_cfg.WindowX0 = 0;
		layer_cfg.WindowX1 = RK043FN48H_WIDTH;
		layer_cfg.WindowY0 = 0;
		layer_cfg.WindowY1 = RK043FN48H_HEIGHT;
		layer_cfg.PixelFormat = LTDC_PIXEL_FORMAT_ARGB8888;
		layer_cfg.FBStartAdress = (uint32_t) activeAddress;
		layer_cfg.Alpha = 255;
		layer_cfg.Alpha0 = 0;
		layer_cfg.Backcolor.Blue = 0;
		layer_cfg.Backcolor.Green = 0;
		layer_cfg.Backcolor.Red = 0;
		layer_cfg.BlendingFactor1 = LTDC_BLENDING_FACTOR1_PAxCA;
		layer_cfg.BlendingFactor2 = LTDC_BLENDING_FACTOR2_PAxCA;
		layer_cfg.ImageWidth = RK043FN48H_WIDTH;
		layer_cfg.ImageHeight = RK043FN48H_HEIGHT;

		HAL_LTDC_ConfigLayer(&hLtdcHandler, &layer_cfg, 0);

		DrawAddress = drawAddress;
	}

	void LCD_DeInit(void)
	{
		/* Initialize the hLtdcHandler Instance parameter */
		hLtdcHandler.Instance = LTDC;

		/* Disable LTDC block */
		__HAL_LTDC_DISABLE(&hLtdcHandler);

		/* DeInit the LTDC */
		HAL_LTDC_DeInit(&hLtdcHandler);

		GPIO_InitTypeDef  gpio_init_structure;

		/* Disable LTDC block */
		__HAL_LTDC_DISABLE(&hLtdcHandler);

		/* LTDC Pins deactivation */

		/* GPIOE deactivation */
		gpio_init_structure.Pin = GPIO_PIN_4;
		HAL_GPIO_DeInit(GPIOE, gpio_init_structure.Pin);

		/* GPIOG deactivation */
		gpio_init_structure.Pin = GPIO_PIN_12;
		HAL_GPIO_DeInit(GPIOG, gpio_init_structure.Pin);

		/* GPIOI deactivation */
		gpio_init_structure.Pin = GPIO_PIN_8 | GPIO_PIN_9 | GPIO_PIN_10 | GPIO_PIN_12 | \
			GPIO_PIN_13 | GPIO_PIN_14 | GPIO_PIN_15;
		HAL_GPIO_DeInit(GPIOI, gpio_init_structure.Pin);

		/* GPIOJ deactivation */
		gpio_init_structure.Pin = GPIO_PIN_0 | GPIO_PIN_1 | GPIO_PIN_2 | GPIO_PIN_3 | \
			GPIO_PIN_4 | GPIO_PIN_5 | GPIO_PIN_6 | GPIO_PIN_7 | \
			GPIO_PIN_8 | GPIO_PIN_9 | GPIO_PIN_10 | GPIO_PIN_11 | \
			GPIO_PIN_13 | GPIO_PIN_14 | GPIO_PIN_15;
		HAL_GPIO_DeInit(GPIOJ, gpio_init_structure.Pin);

		/* GPIOK deactivation */
		gpio_init_structure.Pin = GPIO_PIN_0 | GPIO_PIN_1 | GPIO_PIN_2 | GPIO_PIN_4 | \
			GPIO_PIN_5 | GPIO_PIN_6 | GPIO_PIN_7;
		HAL_GPIO_DeInit(GPIOK, gpio_init_structure.Pin);

		/* Disable LTDC clock */
		__HAL_RCC_LTDC_CLK_DISABLE();
	}
	
	void LCD_Dma2D(uint32_t x, uint32_t y, int width, int height) {
		DMA2D->OPFCCR = CM_ARGB8888;
		DMA2D->OCOLR = 0x00;
		DMA2D->OMAR = (uint32_t)&DrawAddress[RK043FN48H_WIDTH * y + x];
		DMA2D->OOR = RK043FN48H_WIDTH - width;
		DMA2D->NLR = (uint32_t)((width << 16) | height);
		DMA2D->CR |= 1;

		while (DMA2D->CR & DMA2D_CR_START) { 
		//	__WFI(); 
		}
	}

	void LCD_DrawImageWithAlpha(uint32_t* image, int srcX, int srcY, int srcWidth, int dstX, int dstY, int dstWidth, int dstHeight) {
		DMA2D->FGPFCCR = CM_ARGB8888;
		DMA2D->FGCOLR = 0;
		DMA2D->FGMAR = (uint32_t)&image[srcWidth * srcY + srcX];
		DMA2D->FGOR = srcWidth - dstWidth;
		DMA2D->BGPFCCR = CM_ARGB8888;
		DMA2D->BGCOLR = 0;
		DMA2D->BGMAR = (uint32_t)&DrawAddress[RK043FN48H_WIDTH * dstY + dstX];
		DMA2D->BGOR = RK043FN48H_WIDTH - dstWidth;
		DMA2D->CR = DMA2D_M2M_BLEND;

		LCD_Dma2D(dstX, dstY, dstWidth, dstHeight);
	}

	void LCD_SetDrawAddress(uint32_t *address)
	{
		DrawAddress = address;
	}

	void LCD_SetActiveAddress(uint32_t *address)
	{
		HAL_LTDC_SetAddress(&hLtdcHandler, (uint32_t) address, 0);
	}
}