using UnityEngine;
using System.Collections;

public static class _GUI_
{
	/** Static Members **/
	public static readonly float Sw;
	public static readonly float Sh;
	public static readonly float MenuBW;
	public static readonly float MenuBH;
	public static readonly float ScoreW;
	public static readonly float ScoreH;
	public static readonly float MenuItemW;
	public static readonly float MenuItemH;
	public static readonly float CDRectW;
	public static readonly float CDRectH;
	public static readonly Rect fullscreenRect;
	public static readonly Rect beginGameRect;
	public static readonly Rect onePlayerButtonRect;
	public static readonly Rect twoPlayerButtonRect;
	public static readonly Rect p1IDTextEntryRect;
	public static readonly Rect p2IDTextEntryRect;
	public static readonly Rect submitParticInfoRect;
	public static readonly Rect HumanClient1ButtonRect;
	public static readonly Rect HumanClient2ButtonRect;
	public static readonly Rect PongServerButtonRect;
	public static readonly Rect Menu_SessionNumberLabelRect;
	public static readonly Rect Menu_SessionNumberInputRect;
	public static readonly Rect Menu_SessionNameLabelRect;
	public static readonly Rect Menu_ServerIPEntryLabelRect;
	public static readonly Rect Menu_ServerIPEntryInputRect;
	public static readonly Rect Menu_ClientPort1LabelRect;
	public static readonly Rect Menu_ClientPort1InputRect;
	public static readonly Rect Menu_ClientPort2LabelRect;
	public static readonly Rect Menu_ClientPort2InputRect;
	public static readonly Rect Menu_InputMethodLabelRect;
	public static readonly Rect Menu_InputMethodInputRect;
	public static readonly Rect Menu_NumPCLabelRect;
	public static readonly Rect Menu_NumPCInputRect;
	public static readonly Rect Menu_SubmitButtonRect;
	public static readonly Rect Menu_P1IDLabelRect;
	public static readonly Rect Menu_P1IDInputRect;
	public static readonly Rect Menu_P2IDLabelRect;
	public static readonly Rect Menu_P2IDInputRect;
	public static readonly Rect Menu_P1HrsLabelRect;
	public static readonly Rect Menu_P1HrsInputRect;
	public static readonly Rect Menu_P2HrsLabelRect;
	public static readonly Rect Menu_P2HrsInputRect;
	public static readonly Rect NetComm_StatusRect;
	public static readonly Rect CountDownRect;
	public static readonly Rect FeedbackRect;
	public static readonly Rect ClientDebugRect;
	public static readonly ScoreRect leftScoreRect;
	public static readonly ScoreRect rightScoreRect;
	public static readonly ScoreRect rallyScoreRect;

	/**
	 * Static constructor.
	 */
	static _GUI_()
	{
		Sw = Screen.width;
		Sh = Screen.height;
		MenuBW = Sw * 0.10F;
		MenuBH = Sh * 0.10F;
		ScoreW = Sw * 0.15F;
		ScoreH = Sh * 0.20F;
		MenuItemW = Sw / 3F;
		MenuItemH = Sh / 3F;
		CDRectW = Sw * 0.8F;
		CDRectH = Sh * 0.8F;

		fullscreenRect = new Rect (0,0,Sw,Sh);
		beginGameRect = new Rect (.5F*Sw - .5F*MenuBW, .5F*Sh - .5F*MenuBH, MenuBW, MenuBH);
		onePlayerButtonRect = new Rect (.5F*Sw - MenuBW,.5F*Sh - .5F*MenuBH,MenuBW,MenuBH);
		twoPlayerButtonRect = new Rect (.5F*Sw,.5F*Sh - .5F*MenuBH,MenuBW,MenuBH);
		p1IDTextEntryRect = new Rect (.5F*Sw - .5F*MenuBW, .5F*Sh - .5F*MenuBH, MenuBW, MenuBH);
		p2IDTextEntryRect = new Rect (.5F*Sw - .5F*MenuBW, .5F*Sh + .5F*MenuBH, MenuBW, MenuBH);
		submitParticInfoRect = new Rect (.5F*Sw - .5F*MenuBW, .5F*Sh + 1.5F*MenuBH, MenuBW, MenuBH);
		HumanClient1ButtonRect = new Rect (.5F*Sw - MenuBW,.5F*Sh - .5F*MenuBH,MenuBW,MenuBH);
		HumanClient2ButtonRect = new Rect (.5F*Sw - MenuBW,.5F*Sh - .5F*MenuBH+MenuBH,MenuBW,MenuBH);
		PongServerButtonRect = new Rect (.5F*Sw - MenuBW,.5F*Sh - .5F*MenuBH+2F*MenuBH,MenuBW,MenuBH);

		Menu_SessionNumberLabelRect = new Rect (0,0,MenuItemW,MenuItemH);
		Menu_SessionNumberInputRect = new Rect (0,50,100F,50F);
		Menu_SessionNameLabelRect = new Rect (0,24F,MenuItemW,MenuItemH);
		Menu_NumPCLabelRect = new Rect (0,MenuItemH,MenuItemW,MenuItemH);
		Menu_NumPCInputRect = new Rect (0,MenuItemH+50,100F,50F);
		Menu_InputMethodLabelRect = new Rect (0,2F*MenuItemH,MenuItemW,MenuItemH);
		Menu_InputMethodInputRect = new Rect (0,2F*MenuItemH+50,100,50);
		Menu_ServerIPEntryLabelRect = new Rect (MenuItemW*0.50F,0,MenuItemW,MenuItemH);
		Menu_ServerIPEntryInputRect = new Rect (MenuItemW*0.50F,50,100,50);
		Menu_ClientPort1LabelRect = new Rect (MenuItemW*0.50F,MenuItemH,MenuItemW,MenuItemH);
		Menu_ClientPort1InputRect = new Rect (MenuItemW*0.50F,MenuItemH+50,100,50);
		Menu_ClientPort2LabelRect = new Rect (MenuItemW*0.50F,2F*MenuItemH,MenuItemW,MenuItemH);
		Menu_ClientPort2InputRect = new Rect (MenuItemW*0.50F,2F*MenuItemH+50,100,50);
		Menu_P1IDLabelRect = new Rect (MenuItemW,0,MenuItemW,MenuItemH);
		Menu_P1IDInputRect = new Rect (MenuItemW,50,100,50);
		Menu_P2IDLabelRect = new Rect (MenuItemW,MenuItemH,MenuItemW,MenuItemH);
		Menu_P2IDInputRect = new Rect (MenuItemW,MenuItemH+50,100,50);
		Menu_P1HrsLabelRect = new Rect (MenuItemW*1.25F,0,MenuItemW,MenuItemH);
		Menu_P1HrsInputRect = new Rect (MenuItemW*1.25F,50,100,50);
		Menu_P2HrsLabelRect = new Rect (MenuItemW*1.25F,MenuItemH,MenuItemW,MenuItemH);
		Menu_P2HrsInputRect = new Rect (MenuItemW*1.25F,MenuItemH+50,100,50);
		Menu_SubmitButtonRect = new Rect (2*MenuItemW, Sh/2F - 0.5F*MenuBH, MenuBW, MenuBH);

		NetComm_StatusRect = new Rect ( 0,0,100F,35F );
		CountDownRect = new Rect ( (Sw/2F)-(CDRectW/2F),(Sh/2F)-(CDRectH/2F),CDRectW,CDRectH );
		FeedbackRect = new Rect (Sw*0.05F, Sh*0.05F, Sw*0.9F, Sh*0.9F);
		ClientDebugRect = new Rect (0,Sh*0.9F,Sw,Sh*0.1F);

		leftScoreRect = new ScoreRect (ScoreRect.TYPE_LEFT_PADDLE,
		                               "Gold Star",
		                               "Gold Star - Absent");
		rightScoreRect = new ScoreRect (ScoreRect.TYPE_RIGHT_PADDLE,
		                                "Gold Star",
		                                "Gold Star - Absent");
		rallyScoreRect = new ScoreRect (ScoreRect.TYPE_RALLY,
		                                "Gold Star",
		                                "Gold Star - Absent");
	}

	/**
	 * This function wraps a string in style tags for the score.
	 */
	public static string FormatScoreText( string score )
	{
		string pre = "<size=120><b>";
		string post = "</b></size>";

		return pre + score + post;
	}

	/**
	 * This function wraps a string in style tags for feedback text.
	 */
	public static string FormatFeedbackText( string feedback, string color )
	{
		string pre = "<size=50><b><color="+color+">";
		string post = "</color></b></size>";
		
		return pre + feedback + post;
	}

	/**
	 * This function wraps a string in style tags for client debugging text.
	 */
	public static string FormatClientDebugText( string text )
	{
		string pre = "<size=24><b>";
		string post = "</b></size>";
		
		return pre + text + post;
	}

	/**
	 * This function returns the Rect for difficulty level i.
	 */
	public static Rect DiffLevelButtonRect( int i )
	{
		float w = 0, h = 0, x = 0, y = 0;
		float n = 3F; //number of difficulty levels
		y = .5F * (Sh - MenuBH);
		w = MenuBW;
		h = MenuBH;
		x = (n * w - 1F) / (2F) * ((float)(i + 1)) + ((-n * w) + .5F * (Sw - 1F));
		x -= .5F * w;
		return new Rect (x, y, w, h);
	}
}