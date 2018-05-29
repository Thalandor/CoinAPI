pragma solidity ^0.4.24;

interface token {
    function transfer(address receiver, uint amount) external;
}

contract Crowdsale {
    address public beneficiary;
    uint public fundingGoal;
    uint public amountRaised;
    uint public price;
    token public tokenReward;
    uint public tokenAmount ;
    mapping(address => uint256) public balanceOf;
    bool fundingGoalReached = false;
    bool crowdsaleClosed = false;

    event FundTransfer(address backer, uint amount);
    event FinishICO(address owner, uint amount);

    /**
     * Constructor function
     *
     * Setup the owner
     */
    constructor(
        uint tokensToSell,
        address ifSuccessfulSendTo,
        uint etherCostOfEachToken,
        address addressOfTokenUsedAsReward
    ) public {
        tokenAmount = tokensToSell;
        beneficiary = ifSuccessfulSendTo;
        price = etherCostOfEachToken * 1 ether;
        tokenReward = token(addressOfTokenUsedAsReward);
    }

    /**
     * Fallback function
     *
     * The function without name is the default function that is called whenever anyone sends funds to a contract
     */
    function () public payable {
        require(!crowdsaleClosed);
        uint amount = msg.value;
        balanceOf[msg.sender] += amount;
        amountRaised += amount;
        tokenReward.transfer(msg.sender, amount / price);
        emit FundTransfer(msg.sender, amount);
    }

    function finishSale() public {
        crowdsaleClosed = true;
        if (beneficiary.send(amountRaised)) {
                emit FinishICO(beneficiary, amountRaised);
        } 
    }
}