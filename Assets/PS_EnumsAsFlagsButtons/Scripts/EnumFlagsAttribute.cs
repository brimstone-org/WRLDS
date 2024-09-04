using UnityEngine;

public class EnumFlagsAttribute : PropertyAttribute
{
	public int numBtnsPerRow;
    public bool showTooltip;
    public bool showLabel;

	public EnumFlagsAttribute() {
		this.numBtnsPerRow = -1; // Fit as many as possible
        this.showTooltip = true;
        this.showLabel = true;
	}

	public EnumFlagsAttribute(int numBtnsPerRow) {
		this.numBtnsPerRow = numBtnsPerRow;
        this.showTooltip = true;
        this.showLabel = true;
    }

    public EnumFlagsAttribute(int numBtnsPerRow, bool showTooltip) {
        this.numBtnsPerRow = numBtnsPerRow;
        this.showTooltip = showTooltip;
        this.showLabel = true;
    }

    public EnumFlagsAttribute(int numBtnsPerRow, bool showTooltip, bool showLabel) {
        this.numBtnsPerRow = numBtnsPerRow;
        this.showTooltip = showTooltip;
        this.showLabel = showLabel;
    }
}